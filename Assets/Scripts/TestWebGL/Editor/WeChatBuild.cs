using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace SaveWorld.Editor
{
    /// <summary>
    /// 微信小游戏构建自动化脚本（团结引擎 MiniGame 平台）
    /// 使用方法: Unity Editor 菜单 → WeChat → Build MiniGame
    /// 命令行: -executeMethod SaveWorld.Editor.WeChatBuild.Build
    /// </summary>
    public static class WeChatBuild
    {
        private const string BuildFolder = "Builds/WebGL";
        private const string MinigameFolder = "Builds/WebGL/minigame";

        [MenuItem("WeChat/Build MiniGame", false, 100)]
        public static void Build()
        {
            Debug.Log("========== 开始微信小游戏 MiniGame 构建 ==========");

            // 1. 确保输出目录存在
            EnsureDirectory(BuildFolder);

            // 2. 检查 WeChatConfig
            CheckWeChatConfig();

            // 3. 获取构建场景
            string[] scenes = GetBuildScenes();

            // 4. 配置 Player Settings
            ConfigurePlayerSettings();

            // 4.1 校验 MiniGameConfig 关键配置
            ValidateMiniGameConfig();

            // 5. 执行 MiniGame 构建
            BuildReport report = BuildPipeline.BuildPlayer(
                scenes,
                BuildFolder,
                BuildTarget.WeixinMiniGame,
                BuildOptions.None
            );

            // 6. 检查构建结果
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"✅ 构建成功! 输出目录: {Path.GetFullPath(BuildFolder)}");
                Debug.Log($"   总大小: {FormatSize(GetDirectorySize(BuildFolder))}");
                Debug.Log($"   耗时: {report.summary.totalTime.TotalSeconds:F1} 秒");

                // 7. 验证 minigame 目录
                if (Directory.Exists(MinigameFolder))
                {
                    // 检查关键文件确认结构完整
                    string[] requiredFiles = { "game.json", "game.js", "project.config.json" };
                    bool allFilesExist = true;
                    foreach (var file in requiredFiles)
                    {
                        if (!File.Exists(Path.Combine(MinigameFolder, file)))
                        {
                            Debug.LogWarning($"⚠️ minigame 缺少关键文件: {file}");
                            allFilesExist = false;
                        }
                    }

                    if (allFilesExist)
                    {
                        Debug.Log("✅ minigame 目录已生成（结构完整）");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ minigame 目录存在但文件不完整，建议重新构建");
                    }
                    Debug.Log("下一步: 用微信开发者工具打开 Builds/WebGL/minigame/");
                }
                else
                {
                    Debug.LogError("❌ minigame 目录未生成！可能原因:");
                    Debug.LogError("   1. MiniGameConfig.asset 中 dstMinDir 为空 → 在 WX 菜单中设置或手动编辑 .asset 文件");
                    Debug.LogError("   2. 微信 SDK PostProcessBuild 未执行 → 检查 Console 是否有 SDK 错误");
                    Debug.LogError("   3. 构建目标不是 WeixinMiniGame → 确认 Build Settings 平台选择正确");
                }

                EditorUtility.RevealInFinder(BuildFolder);
            }
            else
            {
                Debug.LogError($"❌ 构建失败! 错误数: {report.summary.totalErrors}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            Debug.LogError($"   {msg.content}");
                        }
                    }
                }
            }

            Debug.Log("========== 构建完成 ==========");
        }

        [MenuItem("WeChat/Check Config", false, 200)]
        public static void CheckConfig()
        {
            CheckWeChatConfig();
        }

        [MenuItem("WeChat/Open Build Folder", false, 300)]
        public static void OpenBuildFolder()
        {
            EnsureDirectory(BuildFolder);
            EditorUtility.RevealInFinder(BuildFolder);
        }

        /// <summary>
        /// 校验 MiniGameConfig.asset 中的关键配置，防止 dstMinDir 为空导致 minigame 目录缺失
        /// </summary>
        private static void ValidateMiniGameConfig()
        {
            // 加载 SDK 的 MiniGameConfig
            var configGuid = AssetDatabase.FindAssets("t:MiniGameConfig")[0];
            if (string.IsNullOrEmpty(configGuid))
            {
                Debug.LogWarning("⚠️ 未找到 MiniGameConfig.asset，无法校验 SDK 配置");
                return;
            }

            string configPath = AssetDatabase.GUIDToAssetPath(configGuid);
            var configAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(configPath);
            if (configAsset == null) return;

            // 通过 SerializedObject 读取 dstMinDir 字段
            var so = new SerializedObject(configAsset);
            var dstMinDir = so.FindProperty("ProjectConf.dstMinDir");

            if (dstMinDir != null && string.IsNullOrEmpty(dstMinDir.stringValue))
            {
                Debug.LogError("❌ MiniGameConfig.dstMinDir 为空！这将导致 minigame 目录无法生成。");
                Debug.LogError("   修复方法: 在 Unity 中打开 WX → Mini Game Config，设置 DST Min Dir 为 'minigame'");
            }
            else if (dstMinDir != null)
            {
                Debug.Log($"✅ MiniGameConfig.dstMinDir = '{dstMinDir.stringValue}'");
            }
        }

        private static void CheckWeChatConfig()
        {
            var config = Resources.Load<SaveWorld.Game.WeChat.WeChatConfig>("WeChatConfig");
            if (config == null)
            {
                Debug.LogWarning("⚠️ 未找到 Resources/WeChatConfig.asset！\n" +
                               "请在 Project 窗口右键 → Create → WeChat → Config 创建配置文件，" +
                               "并放入 Assets/Resources/ 目录。\n" +
                               "当前将使用内置默认值（广告/支付功能不可用）。");
            }
            else
            {
                Debug.Log($"📋 WeChatConfig 状态:\n{config.GetConfigSummary()}");
            }
        }

        private static string[] GetBuildScenes()
        {
            var scenes = EditorBuildSettings.scenes;
            var scenePaths = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenePaths[i] = scenes[i].path;
            }
            return scenePaths.Length > 0 ? scenePaths : new[] { "Assets/Scenes/Main.unity" };
        }

        private static void ConfigurePlayerSettings()
        {
            // 通用设置
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WeixinMiniGame, ManagedStrippingLevel.Medium);

            // MiniGame 特定设置（团结引擎 API）
            // 异常支持：关闭以减小包体（调试时在 Player Settings 中手动开启）
            PlayerSettings.MiniGame.exceptionSupport = MiniGameExceptionSupport.None;

            // 压缩：Brotli 减小包体
            PlayerSettings.MiniGame.compressionFormat = MiniGameCompressionFormat.Brotli;

            // 内存
            PlayerSettings.MiniGame.memorySize = 256;

            // 关闭不需要的功能
            PlayerSettings.MiniGame.dataCaching = false;
            PlayerSettings.MiniGame.linkerTarget = MiniGameLinkerTarget.Wasm;

            Debug.Log("✅ Player Settings 配置完成 (MiniGame)");
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"📁 创建目录: {path}");
            }
        }

        private static long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            long size = 0;
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                size += new FileInfo(file).Length;
            }
            return size;
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        }
    }
}
