"""
DeepSeek Agent - 用 Playwright 控制 chat.deepseek.com 网页
实现读文件 / 执行命令 / 列文件 的 AI Agent 循环

用法:
    pip install playwright
    playwright install chromium
    python agent.py [工作区目录]
"""

import asyncio
import glob
import json
import os
import re
import subprocess
import sys
import base64  # 添加这行！
from playwright.async_api import async_playwright, Page

# ── 工具定义 ──────────────────────────────────────────────────────────────────

TOOL_DESCRIPTIONS = r"""
你是一个代码助手，可以使用以下工具读取文件、执行命令和修改文件。

**重要：当需要使用工具时，必须在单独一行输出，不能有任何其他文字！**

正确格式（单独一行）：
TOOL_CALL: {{"tool": "read_file", "path": "相对路径"}}

错误格式（有额外文字）：
我来读取文件。TOOL_CALL: {{"tool": "read_file", "path": "相对路径"}}

可用工具：
- read_file: 读取文件，参数 path
- write_file: 写入文件，参数 path, content_b64 (Base64编码的内容)
- replace_in_file: 替换文件内容，参数 path, old_b64, new_b64
- run_bash: 执行命令，参数 command_b64
- list_files: 列出文件，参数 pattern, directory(可选)

**规则**：
1. 路径必须使用正斜杠 /，例如：Assets/Scripts/Game/Order/OrderEngine.cs
2. 文件内容和命令必须使用 Base64 编码
3. TOOL_CALL 必须单独占一行，前后不能有其他文字
4. 每次只能输出一个 TOOL_CALL（如需多个工具，依次输出）
5. 输出 TOOL_CALL 后立即停止，等待工具结果

工作区根目录: {workspace}
"""


def execute_tool(tool_call: dict, workspace: str) -> str:
    tool = tool_call.get("tool", "")

    if tool == "read_file":
        path = tool_call.get("path", "")
        path = path.replace('\\', '/')
        full = os.path.join(workspace, path) if not os.path.isabs(path) else path
        full = os.path.normpath(full)
        
        try:
            with open(full, "r", encoding="utf-8", errors="replace") as f:
                content = f.read()
            
            # 直接返回可读文本，不需要 Base64
            truncated = content[:15000]
            suffix = f"\n...(截断，共{len(content)}字符)" if len(content) > 15000 else ""
            
            # 添加行号便于 AI 定位
            lines = truncated.split('\n')
            numbered_lines = [f"{i+1:4d}| {line}" for i, line in enumerate(lines)]
            numbered_content = '\n'.join(numbered_lines)
            
            return f"文件内容 [{path}] (共{len(content)}字符):\n```\n{numbered_content}{suffix}\n```"
        except Exception as e:
            return f"读取文件失败 [{path}]: {e}"

    elif tool == "write_file":
        path = tool_call.get("path", "")
        # 支持两种格式：content_b64 (Base64) 或 content (纯文本)
        content_b64 = tool_call.get("content_b64", "")
        content = tool_call.get("content", "")
        
        path = path.replace('\\', '/')
        full = os.path.join(workspace, path) if not os.path.isabs(path) else path
        full = os.path.normpath(full)
        
        if not os.path.abspath(full).startswith(os.path.abspath(workspace)):
            return f"写入失败：路径 [{path}] 不在工作区内"
        
        try:
            # 优先使用 Base64，否则使用纯文本
            if content_b64:
                final_content = base64.b64decode(content_b64).decode('utf-8')
            else:
                final_content = content
            
            os.makedirs(os.path.dirname(full) or '.', exist_ok=True)
            
            with open(full, "w", encoding="utf-8") as f:
                f.write(final_content)
            
            size = os.path.getsize(full)
            lines = final_content.count('\n') + 1
            return f"✓ 文件写入成功 [{path}]\n  大小: {size} 字节, {lines} 行"
        except Exception as e:
            return f"写入文件失败 [{path}]: {e}"

    elif tool == "replace_in_file":
        path = tool_call.get("path", "")
        old_b64 = tool_call.get("old_b64", "")
        old_str = tool_call.get("old_str", "")
        new_b64 = tool_call.get("new_b64", "")
        new_str = tool_call.get("new_str", "")
        
        path = path.replace('\\', '/')
        full = os.path.join(workspace, path) if not os.path.isabs(path) else path
        full = os.path.normpath(full)
        
        if not os.path.abspath(full).startswith(os.path.abspath(workspace)):
            return f"替换失败：路径 [{path}] 不在工作区内"
        
        try:
            # 解码内容
            if old_b64:
                final_old = base64.b64decode(old_b64).decode('utf-8')
            else:
                final_old = old_str
                
            if new_b64:
                final_new = base64.b64decode(new_b64).decode('utf-8')
            else:
                final_new = new_str
            
            with open(full, "r", encoding="utf-8", errors="replace") as f:
                content = f.read()
            
            if final_old not in content:
                # 显示部分内容帮助调试
                preview = final_old[:100] + "..." if len(final_old) > 100 else final_old
                return f"✗ 替换失败 [{path}]：未找到要替换的内容\n  查找的内容: {preview}"
            
            occurrences = content.count(final_old)
            new_content = content.replace(final_old, final_new, 1)
            
            with open(full, "w", encoding="utf-8") as f:
                f.write(new_content)
            
            if occurrences > 1:
                return f"✓ 替换成功 [{path}]：已替换第 1 处匹配（共 {occurrences} 处）"
            else:
                return f"✓ 替换成功 [{path}]"
                
        except Exception as e:
            return f"替换文件失败 [{path}]: {e}"

    elif tool == "run_bash":
        command_b64 = tool_call.get("command_b64", "")
        command = tool_call.get("command", "")
        
        try:
            if command_b64:
                final_command = base64.b64decode(command_b64).decode('utf-8')
            else:
                final_command = command
                
            print(f"  [bash] {final_command}")
            result = subprocess.run(
                final_command, shell=True, cwd=workspace,
                capture_output=True, text=True, timeout=30,
                encoding="utf-8", errors="replace"
            )
            output = result.stdout + result.stderr
            
            if not output:
                return f"命令执行成功 [{final_command}]（无输出）"
            
            truncated = output[:8000]
            suffix = f"\n...(截断，共{len(output)}字符)" if len(output) > 8000 else ""
            return f"命令输出 [{final_command}]:\n```\n{truncated}{suffix}\n```"
        except subprocess.TimeoutExpired:
            return f"命令超时 [{final_command}]"
        except Exception as e:
            return f"命令执行失败: {e}"

    elif tool == "list_files":
        pattern = tool_call.get("pattern", "*")
        directory = tool_call.get("directory", "")
        directory = directory.replace('\\', '/') if directory else ""
        
        root = os.path.join(workspace, directory) if directory else workspace
        root = os.path.normpath(root)
        
        try:
            # 使用 pathlib 更可靠
            from pathlib import Path
            matches = list(Path(root).glob(pattern))
            
            if not matches:
                return f"未找到匹配 [{pattern}] 的文件"
            
            # 格式化输出
            files = []
            for m in matches[:300]:
                rel_path = m.relative_to(workspace)
                size = m.stat().st_size if m.is_file() else 0
                type_indicator = "/" if m.is_dir() else ""
                files.append(f"  {rel_path}{type_indicator} ({size} bytes)")
            
            return f"找到 {len(matches)} 个匹配 [{pattern}] 的文件:\n" + "\n".join(files)
        except Exception as e:
            return f"列文件失败: {e}"

    return f"未知工具: {tool}"


import re

def parse_tool_calls(text: str) -> list[dict]:
    calls = []
    
    # 使用正则表达式匹配 TOOL_CALL: 后面的 JSON
    pattern = r'TOOL_CALL:\s*(\{[^\}]+\})'
    matches = re.findall(pattern, text)
    
    for json_str in matches:
        try:
            calls.append(json.loads(json_str))
            print(f"  [解析] 成功: {json_str[:50]}...")
        except json.JSONDecodeError:
            try:
                # 修复反斜杠
                fixed = json_str.replace('\\', '\\\\')
                calls.append(json.loads(fixed))
                print(f"  [修复] 成功: {fixed[:50]}...")
            except json.JSONDecodeError as e:
                print(f"  [警告] 解析失败: {e}")
                print(f"  [调试] JSON: {json_str}")
    
    return calls


# ── Playwright 操作（保持不变）────────────────────────────────────────────────

INPUT_SELECTORS = [
    "#chat-input",
    "textarea[placeholder]",
    "textarea",
    "div[contenteditable='true'][class*='input']",
    "div[contenteditable='true']",
]

STOP_BTN_SELECTORS = [
    "button[aria-label*='stop' i]",
    "button[aria-label*='停止']",
    "div[class*='stop']",
    "[class*='stop-btn']",
    "[class*='stopButton']",
]

ANSWER_SELECTORS = [
    "div[class*='markdown']",
    "div[class*='message-content']",
    "div[class*='ds-markdown']",
    "div[class*='chat-message'] div[class*='content']",
]


async def find_input(page: Page):
    for sel in INPUT_SELECTORS:
        el = await page.query_selector(sel)
        if el and await el.is_visible():
            return el
    return None


async def send_message(page: Page, text: str):
    inp = await find_input(page)
    if not inp:
        raise RuntimeError("找不到输入框，请确认页面已加载完毕")

    await inp.click()
    await inp.fill("")
    await page.wait_for_timeout(100)
    await inp.fill(text)
    await page.wait_for_timeout(200)
    await inp.press("Enter")


async def is_generating(page: Page) -> bool:
    for sel in STOP_BTN_SELECTORS:
        el = await page.query_selector(sel)
        if el and await el.is_visible():
            return True
    return False


async def get_last_answer(page: Page) -> str:
    for sel in ANSWER_SELECTORS:
        elements = await page.query_selector_all(sel)
        if elements:
            return await elements[-1].inner_text()
    return ""


async def wait_for_response(page: Page, timeout_sec: int = 120) -> str:
    await page.wait_for_timeout(1500)

    last_text = ""
    stable_ticks = 0
    check_interval = 600

    for _ in range(int(timeout_sec * 1000 / check_interval)):
        generating = await is_generating(page)
        current = await get_last_answer(page)

        if not generating:
            if current == last_text and current.strip():
                stable_ticks += 1
                if stable_ticks >= 3:
                    return current
            else:
                stable_ticks = 0
                last_text = current
        else:
            stable_ticks = 0
            last_text = current

        await page.wait_for_timeout(check_interval)

    return last_text


# ── 主 Agent 循环 ──────────────────────────────────────────────────────────────

async def agent_turn(page: Page, workspace: str, user_message: str):
    """处理一轮用户输入，包含内部工具调用循环"""
    await send_message(page, user_message)

    while True:
        response = await wait_for_response(page)
        if not response.strip():
            print("AI: (无回答，可能页面结构变化)")
            break

        # 调试：打印原始响应
        print(f"\n[调试] AI 原始响应:\n{response[:500]}...\n")

        tool_calls = parse_tool_calls(response)

        if not tool_calls:
            # 没有工具调用，打印最终回答
            clean = "\n".join(
                l for l in response.splitlines()
                if "TOOL_CALL:" not in l  # 过滤掉包含 TOOL_CALL 的行
            ).strip()
            if clean:
                print(f"\nAI: {clean}\n")
            break

        print(f"  [执行 {len(tool_calls)} 个工具调用]")
        results = []
        for tc in tool_calls:
            print(f"  [tool] {tc.get('tool')} ...")
            result = execute_tool(tc, workspace)
            results.append(result)
            # 打印工具执行结果的前200字符
            print(f"  [结果] {result[:200]}...")

        feedback = "工具执行结果：\n\n" + "\n\n---\n\n".join(results) + "\n\n请继续分析。"
        await send_message(page, feedback)


async def main():
    workspace = sys.argv[1] if len(sys.argv) > 1 else os.getcwd()
    workspace = os.path.abspath(workspace)
    print(f"工作区: {workspace}")

    profile_dir = os.path.join(os.path.expanduser("~"), ".deepseek_agent_profile")
    print(f"浏览器配置: {profile_dir}（首次登录后下次免登录）\n")

    async with async_playwright() as p:
        context = await p.chromium.launch_persistent_context(
            user_data_dir=profile_dir,
            headless=False,
            args=["--start-maximized"],
            viewport=None,
        )

        page = context.pages[0] if context.pages else await context.new_page()
        await page.goto("https://chat.deepseek.com", wait_until="domcontentloaded")
        await page.wait_for_timeout(2000)

        print("如需登录，请在浏览器中完成，然后回到此处按 Enter 继续...")
        input()

        print("发送系统提示...")
        system_msg = TOOL_DESCRIPTIONS.format(workspace=workspace)
        await send_message(page, system_msg)
        init_resp = await wait_for_response(page)
        print(f"AI 确认: {init_resp[:120].strip()}...\n")

        print("="*50)
        print("Agent 就绪。输入问题，AI 会自动读文件/执行命令。")
        print("输入 exit 退出。")
        print("="*50)

        while True:
            try:
                user_input = input("\nYou: ").strip()
            except (EOFError, KeyboardInterrupt):
                break

            if user_input.lower() in ("exit", "quit", "q", "退出"):
                break
            if not user_input:
                continue

            await agent_turn(page, workspace, user_input)

        await context.close()
        print("已退出。")


if __name__ == "__main__":
    asyncio.run(main())