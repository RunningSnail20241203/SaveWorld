using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

namespace SaveWorld.Editor
{
    /// <summary>
    /// 微信小游戏构建辅助工具。
    /// 构建请使用官方菜单「微信小游戏 → 构建」，构建后 WeChatPostBuildPatch 会自动 patch 产物。
    /// </summary>
    public static class WeChatBuild
    {
        private const string CdnScript = "start-local-cdn.js";
        private const int CdnPort = 18765;

        // ---- 本地 CDN 服务器管理 ----

        private static Process s_CdnProcess;

        [MenuItem("WeChat/Local CDN/Start", false, 400)]
        public static void StartCdn()
        {
            if (IsCdnRunning())
            {
                Debug.Log($"[Local CDN] 端口 {CdnPort} 已在使用中，服务器可能已在运行。");
                return;
            }

            string projectRoot = Path.GetFullPath(".");
            string scriptPath = Path.Combine(projectRoot, CdnScript);

            if (!File.Exists(scriptPath))
            {
                Debug.LogError($"[Local CDN] 未找到 {CdnScript}，请确认文件存在于项目根目录。");
                return;
            }

            string webglDir = Path.Combine(projectRoot, "Builds/webgl");
            if (!Directory.Exists(webglDir))
            {
                Debug.LogWarning($"[Local CDN] 目录 {webglDir} 不存在，请先执行构建。");
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "node",
                    Arguments = $"\"{scriptPath}\" {CdnPort}",
                    WorkingDirectory = projectRoot,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                s_CdnProcess = Process.Start(startInfo);

                // 异步读取输出，避免阻塞
                s_CdnProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        UnityEngine.Debug.Log($"[CDN] {e.Data}");
                };
                s_CdnProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        UnityEngine.Debug.LogError($"[CDN] {e.Data}");
                };
                s_CdnProcess.BeginOutputReadLine();
                s_CdnProcess.BeginErrorReadLine();

                s_CdnProcess.EnableRaisingEvents = true;
                s_CdnProcess.Exited += (sender, e) =>
                {
                    s_CdnProcess = null;
                    // 菜单状态需要在主线程刷新
                    EditorApplication.delayCall += () =>
                    {
                        // 不做额外操作，下次菜单点击时会重新检测
                    };
                };

                Debug.Log($"[Local CDN] 已启动 http://localhost:{CdnPort} (PID: {s_CdnProcess.Id})");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Local CDN] 启动失败: {ex.Message}");
            }
        }

        [MenuItem("WeChat/Local CDN/Stop", false, 401)]
        public static void StopCdn()
        {
            if (s_CdnProcess != null && !s_CdnProcess.HasExited)
            {
                try
                {
                    s_CdnProcess.Kill();
                    s_CdnProcess.WaitForExit(3000);
                    Debug.Log("[Local CDN] 已停止。");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[Local CDN] 停止进程失败: {ex.Message}");
                }
                finally
                {
                    s_CdnProcess = null;
                }
            }
            else
            {
                // 进程引用丢失时，尝试通过端口查找并终止
                KillByPort();
            }
        }

        [MenuItem("WeChat/Local CDN/Restart", false, 402)]
        public static void RestartCdn()
        {
            StopCdn();
            System.Threading.Thread.Sleep(500);
            StartCdn();
        }

        [MenuItem("WeChat/Local CDN/Start", true)]
        [MenuItem("WeChat/Local CDN/Stop", true)]
        [MenuItem("WeChat/Local CDN/Restart", true)]
        public static bool ValidateCdnMenu()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        /// <summary>
        /// 检查 CDN 端口是否已被占用（简易检测，适用于本场景）。
        /// </summary>
        private static bool IsCdnRunning()
        {
            try
            {
                // 通过 netstat 检查端口是否在监听
                var psi = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-ano | findstr :18765 | findstr LISTENING",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                };
                using var proc = Process.Start(psi);
                string output = proc.StandardOutput.ReadToEnd().Trim();
                return !string.IsNullOrEmpty(output);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 通过端口查找并终止进程（当进程引用丢失时的兜底方案）。
        /// </summary>
        private static void KillByPort()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c for /f \"tokens=5\" %a in ('netstat -ano ^| findstr :18765 ^| findstr LISTENING') do taskkill /F /PID %a",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                using var proc = Process.Start(psi);
                proc.WaitForExit(3000);
                if (proc.ExitCode == 0)
                    Debug.Log("[Local CDN] 已通过端口终止进程。");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Local CDN] 端口清理失败: {ex.Message}");
            }
        }
    }
}
