#if UNITY_EDITOR && !UNITY_WEBGL
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SaveWorld.Editor
{
    /// <summary>
    /// 编译触发器 - 当哨兵文件变化时主动刷新 AssetDatabase 并请求脚本重编译。
    /// 放在 Editor 目录下，仅编辑器环境生效。
    /// </summary>
    [InitializeOnLoad]
    public static class CompileTrigger
    {
        private static string _sentinelPath;
        private static string _lastContent = "";
        private static double _nextCheckTime = 0;
        private const double CHECK_INTERVAL = 2.0; // 每2秒检查一次

        static CompileTrigger()
        {
            _sentinelPath = Path.Combine(
                Application.dataPath,
                "Scripts/TestWebGL/Editor/.compile_trigger"
            );

            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            // 节流：不需要每帧都检查
            var now = EditorApplication.timeSinceStartup;
            if (now < _nextCheckTime) return;
            _nextCheckTime = now + CHECK_INTERVAL;

            if (!File.Exists(_sentinelPath)) return;

            try
            {
                var content = File.ReadAllText(_sentinelPath);
                if (content == _lastContent) return; // 内容没变则跳过

                _lastContent = content;

                Debug.Log($"[CompileTrigger] 检测到编译触发信号，开始刷新...");

                // 删除哨兵文件（避免重复触发）
                try { File.Delete(_sentinelPath); } catch { }

                // 强制导入资源并触发脚本编译
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                #if UNITY_2020_1_OR_NEWER
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                #endif
            }
            catch (System.Exception ex)
            {
                // 文件可能被占用等异常，静默处理
                Debug.LogWarning($"[CompileTrigger] 异常: {ex.Message}");
            }
        }
    }
}
#endif
