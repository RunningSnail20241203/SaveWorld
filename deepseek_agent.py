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
from playwright.async_api import async_playwright, Page

# ── 工具定义 ──────────────────────────────────────────────────────────────────

TOOL_DESCRIPTIONS = r"""
你是一个代码助手，可以使用以下工具读取文件和执行命令。

当需要使用工具时，**单独一行**输出（不要加其他内容）：
  TOOL_CALL: {{"tool": "read_file", "path": "相对路径或绝对路径"}}
  TOOL_CALL: {{"tool": "run_bash", "command": "shell命令，如 grep -r 'xxx' ."}}
  TOOL_CALL: {{"tool": "list_files", "pattern": "**/*.lua", "directory": "可选子目录"}}

**重要**：路径中的反斜杠 \ 必须写为 \\，例如：
  错误：{{"tool": "read_file", "path": "Assets\Scripts\file.cs"}}
  正确：{{"tool": "read_file", "path": "Assets\\Scripts\\file.cs"}}
  或者直接使用正斜杠：{{"tool": "read_file", "path": "Assets/Scripts/file.cs"}}

规则：
- 每次只输出 TOOL_CALL 行，不要附加说明文字
- 输出 TOOL_CALL 后停止，等待工具结果返回
- 收到工具结果后继续分析，直到能给出最终答案
- 最终答案不包含任何 TOOL_CALL 行

工作区根目录: {workspace}
"""


def execute_tool(tool_call: dict, workspace: str) -> str:
    tool = tool_call.get("tool", "")

    if tool == "read_file":
        path = tool_call.get("path", "")
        full = os.path.join(workspace, path) if not os.path.isabs(path) else path
        try:
            with open(full, "r", encoding="utf-8", errors="replace") as f:
                content = f.read()
            truncated = content[:15000]
            suffix = f"\n...(截断，共{len(content)}字符)" if len(content) > 15000 else ""
            return f"文件内容 [{path}]:\n```\n{truncated}{suffix}\n```"
        except Exception as e:
            return f"读取文件失败 [{path}]: {e}"

    elif tool == "run_bash":
        command = tool_call.get("command", "")
        print(f"  [bash] {command}")
        try:
            result = subprocess.run(
                command, shell=True, cwd=workspace,
                capture_output=True, text=True, timeout=30,
                encoding="utf-8", errors="replace"
            )
            output = result.stdout + result.stderr
            truncated = output[:8000]
            suffix = f"\n...(截断)" if len(output) > 8000 else ""
            return f"命令输出 [{command}]:\n```\n{truncated}{suffix}\n```"
        except subprocess.TimeoutExpired:
            return f"命令超时 [{command}]"
        except Exception as e:
            return f"命令执行失败: {e}"

    elif tool == "list_files":
        pattern = tool_call.get("pattern", "*")
        directory = tool_call.get("directory")
        root = os.path.join(workspace, directory) if directory else workspace
        try:
            matches = glob.glob(pattern, root_dir=root, recursive=True)
            if not matches:
                return f"没有匹配 [{pattern}] 的文件"
            return f"匹配 [{pattern}] 的文件 ({len(matches)} 个):\n" + "\n".join(matches[:300])
        except Exception as e:
            return f"列文件失败: {e}"

    return f"未知工具: {tool}"


def parse_tool_calls(text: str) -> list[dict]:
    calls = []
    for line in text.splitlines():
        line = line.strip()
        if line.startswith("TOOL_CALL:"):
            json_str = line[len("TOOL_CALL:"):].strip()
            try:
                calls.append(json.loads(json_str))
            except json.JSONDecodeError as e:
                # 尝试修复未转义的反斜杠
                try:
                    # 将单个反斜杠替换为双反斜杠
                    fixed_json = json_str.replace('\\', '\\\\')
                    calls.append(json.loads(fixed_json))
                    print(f"  [修复] 自动修复路径转义: {json_str[:50]}...")
                except json.JSONDecodeError:
                    print(f"  [警告] 解析 TOOL_CALL 失败: {e} | 原文: {json_str}")
    return calls


# ── Playwright 操作 ────────────────────────────────────────────────────────────

# 输入框选择器（按优先级尝试）
INPUT_SELECTORS = [
    "#chat-input",
    "textarea[placeholder]",
    "textarea",
    "div[contenteditable='true'][class*='input']",
    "div[contenteditable='true']",
]

# 停止生成按钮（出现时表示正在生成，消失时表示完成）
STOP_BTN_SELECTORS = [
    "button[aria-label*='stop' i]",
    "button[aria-label*='停止']",
    "div[class*='stop']",
    "[class*='stop-btn']",
    "[class*='stopButton']",
]

# AI 回答内容区域（取最后一个）
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
    # 清空再填写
    await inp.fill("")
    await page.wait_for_timeout(100)
    await inp.fill(text)
    await page.wait_for_timeout(200)
    await inp.press("Enter")


async def is_generating(page: Page) -> bool:
    """判断 AI 是否仍在生成回答"""
    for sel in STOP_BTN_SELECTORS:
        el = await page.query_selector(sel)
        if el and await el.is_visible():
            return True
    return False


async def get_last_answer(page: Page) -> str:
    """获取页面上最后一条 AI 回答的文本"""
    for sel in ANSWER_SELECTORS:
        elements = await page.query_selector_all(sel)
        if elements:
            return await elements[-1].inner_text()
    # fallback：取页面全文中最后一段
    return ""


async def wait_for_response(page: Page, timeout_sec: int = 120) -> str:
    """等待 AI 回答完成，返回回答文本"""
    # 先等一下让请求发出去
    await page.wait_for_timeout(1500)

    last_text = ""
    stable_ticks = 0
    check_interval = 600  # ms

    for _ in range(int(timeout_sec * 1000 / check_interval)):
        generating = await is_generating(page)
        current = await get_last_answer(page)

        if not generating:
            if current == last_text and current.strip():
                stable_ticks += 1
                if stable_ticks >= 3:  # 稳定 ~1.8s 且不在生成中
                    return current
            else:
                stable_ticks = 0
                last_text = current
        else:
            stable_ticks = 0
            last_text = current

        await page.wait_for_timeout(check_interval)

    return last_text  # 超时返回当前内容


# ── 主 Agent 循环 ──────────────────────────────────────────────────────────────

async def agent_turn(page: Page, workspace: str, user_message: str):
    """处理一轮用户输入，包含内部工具调用循环"""
    await send_message(page, user_message)

    while True:
        response = await wait_for_response(page)
        if not response.strip():
            print("AI: (无回答，可能页面结构变化)")
            break

        tool_calls = parse_tool_calls(response)

        if not tool_calls:
            # 没有工具调用，打印最终回答
            clean = "\n".join(
                l for l in response.splitlines()
                if not l.strip().startswith("TOOL_CALL:")
            ).strip()
            print(f"\nAI: {clean}\n")
            break

        # 执行工具，把结果发回
        print(f"  [执行 {len(tool_calls)} 个工具调用]")
        results = []
        for tc in tool_calls:
            print(f"  [tool] {tc.get('tool')} ...")
            result = execute_tool(tc, workspace)
            results.append(result)

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

        # 初始化：发送工具说明（system prompt）
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
