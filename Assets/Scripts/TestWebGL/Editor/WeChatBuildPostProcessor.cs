using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;

namespace SaveWorld.Editor
{
    /// <summary>
    /// 微信小游戏构建后自动处理。
    /// 使用 [PostProcessBuild] 回调，无论通过哪种方式构建（官方菜单 / WeChat菜单 / 命令行）都会自动执行。
    /// 包含：JS 补丁、开发模式关闭、开放数据域拷贝、插件占位文件生成等。
    /// </summary>
    public static class WeChatBuildPostProcessor
    {
        /// <summary>
        /// 回调优先级。微信 SDK 的 PostProcessBuild 没有显式指定优先级（默认 0），
        /// 我们用 1 确保在 SDK 的转换完成之后执行。
        /// </summary>
        private const int CallbackPriority = 1;

        [PostProcessBuild(CallbackPriority)]
        public static void OnPostProcessBuild(BuildTarget target, string buildPath)
        {
            // 只处理微信小游戏构建
#if !TUANJIE_1_8_OR_NEWER
            if (target != BuildTarget.WeixinMiniGame) return;
#else
            // 团结引擎可能用不同的枚举值，做平台名称兜底
            if (target.ToString() != "WeixinMiniGame") return;
#endif

            // SDK 会在构建目录下生成 minigame/ 子目录
            string minigameDir = Path.Combine(buildPath, "minigame");

            if (!Directory.Exists(minigameDir))
            {
                // 不是微信小游戏构建或 SDK 转换失败，跳过
                return;
            }

            Debug.Log("🔧 [PostProcessor] 检测到 minigame 目录，开始自动处理...");

            int patched = 0;

            // Patch 1: legacyModuleProp abort 陷阱
            // Emscripten 新版对 Module.arguments/thisProgram/quit 设了 getter 陷阱，
            // UnityPlugin 访问时会触发 abort()。预设安全默认值即可绕过。
            patched += PatchFile(
                Path.Combine(minigameDir, "webgl.wasm.framework.unityweb.js"),
                "// minified.",
                "// minified.\n\n" +
                "// Fix: pre-set legacy Module props to prevent legacyModuleProp abort traps\n" +
                "if(!Module.arguments) Module.arguments = [];\n" +
                "if(!Module.thisProgram) Module.thisProgram = '';\n" +
                "if(!Module.quit) Module.quit = function(){};\n",
                "legacyModuleProp abort trap fix"
            );

            // Patch 2: 关闭开发模式
            patched += PatchFile(
                Path.Combine(minigameDir, "unity-namespace.js"),
                "isDevelopmentBuild: true",
                "isDevelopmentBuild: false",
                "isDevelopmentBuild -> false"
            );

            // Patch 3: 关闭渲染分析日志（仅开发有用，线上产生噪音）
            patched += PatchFile(
                Path.Combine(minigameDir, "unity-namespace.js"),
                "enableRenderAnalysisLog: true",
                "enableRenderAnalysisLog: false",
                "enableRenderAnalysisLog -> false"
            );

            // Patch 4: 关闭优化建议弹框（仅开发有用）
            patched += PatchFile(
                Path.Combine(minigameDir, "unity-namespace.js"),
                "showSuggestModal: true",
                "showSuggestModal: false",
                "showSuggestModal -> false"
            );

            // Patch 5: 拷贝开放数据域模板（好友排行榜）
            patched += CopyOpenData(minigameDir);

            // Patch 6: 生成 plugins/check-update.js 占位文件
            // UnityPlugin 插件会在预编译时自动寻找此文件，缺失会导致 ENOENT 报错
            patched += GeneratePluginPlaceholder(minigameDir);

            // Patch 7: 生成 plugins/screen-adapter.js 屏幕适配入口
            // 微信开发者工具 UnityPlugin 预编译期要求此文件存在，缺失会导致 ENOENT
            patched += GenerateScreenAdapter(minigameDir);

            // Patch 8: game.js Worker "not support" rejection 兜底
            // Worker 线程（workers/response/index.js）执行时可能抛出 Error: not support
            // 该 rejection 冒泡到主线程后触发 WXUncaughtException → Unity abort() 致命弹窗
            // 此补丁通过 wx.onUnhandledRejection 全局拦截，防止误杀
            patched += PatchGameJsWorkerRejection(minigameDir);

            if (patched > 0)
                Debug.Log($"[PostProcessor] 完成，共处理 {patched} 处");
            else
                Debug.Log("[PostProcessor] 无需处理（已就位）");
        }

        /// <summary>
        /// 将微信 SDK 默认的 open-data 开放数据域模板拷贝到构建产物中。
        /// 微信开发者工具在 compileType=game 时要求该目录存在。
        /// </summary>
        private static int CopyOpenData(string minigameDir)
        {
            string destDir = Path.Combine(minigameDir, "open-data");
            if (Directory.Exists(destDir))
            {
                Debug.Log("  open-data: 已存在，跳过");
                // 即使目录已存在，仍需确保 game.json 有 openDataContext
                PatchGameJsonOpenDataContext(minigameDir);
                return 0;
            }

            // 在 PackageCache 中查找微信 SDK 的默认 open-data 模板
            string packageCacheDir = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                "Library", "PackageCache");

            if (!Directory.Exists(packageCacheDir))
            {
                Debug.LogWarning("  open-data: 未找到 Library/PackageCache，跳过");
                return 0;
            }

            string openDataSrc = FindOpenDataTemplate(packageCacheDir);
            if (openDataSrc == null || !Directory.Exists(openDataSrc))
            {
                Debug.LogWarning("  open-data: 未找到微信 SDK 默认模板，跳过");
                return 0;
            }

            // 拷贝目录，排除 .meta 文件
            CopyDirectoryExcludeMeta(openDataSrc, destDir);
            Debug.Log("  open-data: 已拷贝开放数据域模板");

            // 同时确保 game.json 包含 openDataContext 字段
            PatchGameJsonOpenDataContext(minigameDir);
            return 1;
        }

        /// <summary>
        /// 确保 game.json 中存在 "openDataContext": "open-data" 字段。
        /// 微信运行时依赖此字段定位开放数据域代码目录。
        /// </summary>
        private static void PatchGameJsonOpenDataContext(string minigameDir)
        {
            string gameJsonPath = Path.Combine(minigameDir, "game.json");
            if (!File.Exists(gameJsonPath)) return;

            string content = File.ReadAllText(gameJsonPath, System.Text.Encoding.UTF8);
            if (content.Contains("\"openDataContext\"")) return;

            // 在 "plugins" 块之后插入 openDataContext 字段
            int pluginsEnd = content.LastIndexOf("}");
            if (pluginsEnd < 0) return;

            string patched = content.Insert(pluginsEnd + 1, ",\n  \"openDataContext\"            : \"open-data\"");
            File.WriteAllText(gameJsonPath, patched, System.Text.Encoding.UTF8);
            Debug.Log("  game.json: 已添加 openDataContext 字段");
        }

        /// <summary>
        /// 生成 plugins/check-update.js 占位文件。
        /// 微信开发者工具加载 UnityPlugin 时会预编译此文件，缺失会抛 ENOENT。
        /// </summary>
        private static int GeneratePluginPlaceholder(string minigameDir)
        {
            string pluginsDir = Path.Combine(minigameDir, "plugins");
            string placeholder = Path.Combine(pluginsDir, "check-update.js");

            if (File.Exists(placeholder))
            {
                Debug.Log("  plugins/check-update.js: 已存在，跳过");
                return 0;
            }

            Directory.CreateDirectory(pluginsDir);
            File.WriteAllText(placeholder,
                "// UnityPlugin check-update placeholder\n" +
                "// Required by WeChat DevTools plugin system\n",
                System.Text.Encoding.UTF8);
            Debug.Log("  plugins/check-update.js: 已生成");
            return 1;
        }

        /// <summary>
        /// 生成 plugins/screen-adapter.js 屏幕适配入口文件。
        /// 微信开发者工具加载 UnityPlugin 时会预编译此文件，缺失会抛 ENOENT。
        /// 支持 plugin-config.js 中定义的全部 scaleMode。
        /// </summary>
        private static int GenerateScreenAdapter(string minigameDir)
        {
            string pluginsDir = Path.Combine(minigameDir, "plugins");
            string filePath = Path.Combine(pluginsDir, "screen-adapter.js");

            if (File.Exists(filePath))
            {
                Debug.Log("  plugins/screen-adapter.js: 已存在，跳过");
                return 0;
            }

            Directory.CreateDirectory(pluginsDir);

            const string content =
                "// UnityPlugin screen-adapter\n" +
                "// Provides screen adaptation for WeChat minigame\n" +
                "var _sysInfo = wx.getWindowInfo ? wx.getWindowInfo() : wx.getSystemInfoSync();\n" +
                "var screenWidth = _sysInfo.screenWidth;\n" +
                "var screenHeight = _sysInfo.screenHeight;\n" +
                "var pixelRatio = _sysInfo.pixelRatio;\n" +
                "\n" +
                "function adaptScreen(canvas, options) {\n" +
                "    if (!canvas) return;\n" +
                "    var mode = (options && options.mode) || 'SHOW_ALL';\n" +
                "    var designWidth = (options && options.designWidth) || screenWidth;\n" +
                "    var designHeight = (options && options.designHeight) || screenHeight;\n" +
                "    var scaleX = screenWidth / designWidth;\n" +
                "    var scaleY = screenHeight / designHeight;\n" +
                "    var scale = 1;\n" +
                "    switch (mode) {\n" +
                "        case 'EXACT_FIT': scaleX = 1; scaleY = 1; break;\n" +
                "        case 'NO_BORDER': scale = Math.max(scaleX, scaleY); scaleX = scale; scaleY = scale; break;\n" +
                "        case 'SHOW_ALL': scale = Math.min(scaleX, scaleY); scaleX = scale; scaleY = scale; break;\n" +
                "        case 'FIXED_HEIGHT': scaleX = scaleY; break;\n" +
                "        case 'FIXED_WIDTH': scaleY = scaleX; break;\n" +
                "        case 'FIXED_NARROW':\n" +
                "            scale = screenWidth < screenHeight ? Math.max(scaleX, scaleY) : Math.min(scaleX, scaleY);\n" +
                "            scaleX = scale; scaleY = scale; break;\n" +
                "        case 'FIXED_WIDE':\n" +
                "            scale = screenWidth > screenHeight ? Math.max(scaleX, scaleY) : Math.min(scaleX, scaleY);\n" +
                "            scaleX = scale; scaleY = scale; break;\n" +
                "    }\n" +
                "    canvas.width = Math.floor(screenWidth * pixelRatio);\n" +
                "    canvas.height = Math.floor(screenHeight * pixelRatio);\n" +
                "}\n" +
                "\n" +
                "module.exports = {\n" +
                "    adaptScreen: adaptScreen,\n" +
                "    screenWidth: screenWidth,\n" +
                "    screenHeight: screenHeight,\n" +
                "    pixelRatio: pixelRatio\n" +
                "};\n";

            File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
            Debug.Log("  plugins/screen-adapter.js: 已生成");
            return 1;
        }

        /// <summary>
        /// 在 PackageCache 中查找 com.qq.weixin.minigame 包的 open-data 目录。
        /// </summary>
        private static string FindOpenDataTemplate(string packageCacheDir)
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(packageCacheDir, "com.qq.weixin.minigame*"))
                {
                    string candidate = Path.Combine(dir, "Runtime", "wechat-default", "open-data");
                    if (Directory.Exists(candidate))
                        return candidate;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// 递归拷贝目录，跳过 .meta 文件（Unity 编辑器标记，微信小游戏不需要）。
        /// </summary>
        private static void CopyDirectoryExcludeMeta(string sourceDir, string destDir)
        {
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                if (Path.GetExtension(file) == ".meta") continue;
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                Directory.CreateDirectory(destSubDir);
                CopyDirectoryExcludeMeta(subDir, destSubDir);
            }
        }

        /// <summary>
        /// 向 game.js 注入 Worker rejection 全局兜底代码。
        /// Worker 线程中的 "not support" 错误是 SDK 已知的非致命问题，
        /// 但未捕获的 Promise rejection 会冒泡到主线程并触发 WXUncaughtException，
        /// 最终导致 Unity WebGL abort() 致命弹窗（三按钮错误页面）。
        /// </summary>
        private static int PatchGameJsWorkerRejection(string minigameDir)
        {
            string gameJsPath = Path.Combine(minigameDir, "game.js");
            if (!File.Exists(gameJsPath)) return 0;

            string content = File.ReadAllText(gameJsPath, System.Text.Encoding.UTF8);

            // 幂等检查：已包含补丁则跳过
            if (content.Contains("suppressNotSupport")) return 0;

            const string suppressionCode =
                "// 全局兜底：吞咽 Worker 线程中 SDK 已知的非致命 Promise rejection，\n" +
                "// 防止触发 Unity WebGL 的 WXUncaughtException 致命错误弹窗\n" +
                "(function () {\n" +
                "  function suppressNotSupport(reason) {\n" +
                "    if (reason && reason.message === 'not support') {\n" +
                "      console.log('[game.js] suppressed \"not support\" rejection (non-fatal)');\n" +
                "      return true;\n" +
                "    }\n" +
                "    return false;\n" +
                "  }\n" +
                "  // 微信小游戏专用 API\n" +
                "  if (typeof wx !== 'undefined' && wx.onUnhandledRejection) {\n" +
                "    wx.onUnhandledRejection(function (res) {\n" +
                "      if (suppressNotSupport(res.reason)) {\n" +
                "        if (res.preventDefault) res.preventDefault();\n" +
                "      }\n" +
                "    });\n" +
                "  }\n" +
                "  // 标准 Web API（兼容浏览器 / DevTools 模拟）\n" +
                "  if (typeof globalThis !== 'undefined' && globalThis.addEventListener) {\n" +
                "    globalThis.addEventListener('unhandledrejection', function (e) {\n" +
                "      if (suppressNotSupport(e.reason)) e.preventDefault();\n" +
                "    });\n" +
                "  }\n" +
                "})();\n";

            // 在 // @ts-nocheck 之后插入，维持与手动补丁一致的布局
            if (content.StartsWith("// @ts-nocheck"))
            {
                int insertPos = "// @ts-nocheck\n".Length;
                content = content.Insert(insertPos, suppressionCode + "\n");
            }
            else
            {
                // 兜底：如果文件格式异常，直接在开头插入
                content = suppressionCode + "\n" + content;
            }

            File.WriteAllText(gameJsPath, content, System.Text.Encoding.UTF8);
            Debug.Log("  patch: game.js Worker rejection 兜底（wx.onUnhandledRejection）");
            return 1;
        }

        private static int PatchFile(string filePath, string oldText, string newText, string patchName)
        {
            if (!File.Exists(filePath)) return 0;

            string content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            if (!content.Contains(oldText)) return 0;

            content = content.Replace(oldText, newText);
            File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
            Debug.Log($"  patch: {patchName}");
            return 1;
        }
    }
}
