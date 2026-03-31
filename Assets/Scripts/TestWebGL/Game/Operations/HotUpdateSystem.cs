using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace TestWebGL.Game.Operations
{
    /// <summary>
    /// 热更新系统
    /// 负责游戏资源的热更新、版本检查、资源下载等功能
    /// </summary>
    public class HotUpdateSystem : MonoBehaviour
    {
        private static HotUpdateSystem s_instance;
        public static HotUpdateSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("HotUpdateSystem");
                    s_instance = go.AddComponent<HotUpdateSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 热更新配置
        private const string VERSION_KEY = "hot_update_version";
        private const string MANIFEST_KEY = "hot_update_manifest";
        private const string UPDATE_SERVER = "https://your-cdn-server.com/updates/";
        private const float CHECK_INTERVAL = 3600f; // 1小时检查一次

        // 版本信息
        private VersionInfo _localVersion;
        private VersionInfo _remoteVersion;
        
        // 更新清单
        private UpdateManifest _localManifest;
        private UpdateManifest _remoteManifest;
        
        // 下载状态
        private bool _isDownloading = false;
        private float _downloadProgress = 0f;
        private string _currentDownloadFile = "";
        
        // 检查计时器
        private float _checkTimer = 0f;
        
        // 初始化状态
        private bool _isInitialized = false;

        // 事件
        public event Action<bool, string> OnVersionCheckCompleted;
        public event Action<UpdateManifest> OnUpdateAvailable;
        public event Action<float> OnDownloadProgress;
        public event Action<bool, string> OnDownloadCompleted;
        public event Action<string> OnUpdateApplied;

        /// <summary>
        /// 初始化热更新系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[HotUpdate] 初始化热更新系统...");

            // 加载本地版本信息
            LoadLocalVersion();
            LoadLocalManifest();

            // 检查更新
            CheckForUpdates();

            _isInitialized = true;
            Debug.Log("[HotUpdate] 热更新系统初始化完成");
        }

        /// <summary>
        /// 加载本地版本信息
        /// </summary>
        private void LoadLocalVersion()
        {
            try
            {
                if (PlayerPrefs.HasKey(VERSION_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(VERSION_KEY);
                    _localVersion = JsonUtility.FromJson<VersionInfo>(jsonData);
                }

                if (_localVersion == null)
                {
                    _localVersion = new VersionInfo
                    {
                        version = Application.version,
                        buildNumber = 1,
                        lastUpdate = DateTime.Now
                    };
                }

                Debug.Log($"[HotUpdate] 本地版本: {_localVersion.version}.{_localVersion.buildNumber}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HotUpdate] 加载本地版本失败: {ex.Message}");
                _localVersion = new VersionInfo
                {
                    version = Application.version,
                    buildNumber = 1,
                    lastUpdate = DateTime.Now
                };
            }
        }

        /// <summary>
        /// 加载本地更新清单
        /// </summary>
        private void LoadLocalManifest()
        {
            try
            {
                if (PlayerPrefs.HasKey(MANIFEST_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(MANIFEST_KEY);
                    _localManifest = JsonUtility.FromJson<UpdateManifest>(jsonData);
                }

                if (_localManifest == null)
                {
                    _localManifest = new UpdateManifest
                    {
                        version = _localVersion.version,
                        files = new List<UpdateFile>()
                    };
                }

                Debug.Log($"[HotUpdate] 本地清单: {_localManifest.files.Count}个文件");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HotUpdate] 加载本地清单失败: {ex.Message}");
                _localManifest = new UpdateManifest
                {
                    version = _localVersion.version,
                    files = new List<UpdateFile>()
                };
            }
        }

        /// <summary>
        /// 保存本地版本信息
        /// </summary>
        private void SaveLocalVersion()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(_localVersion);
                PlayerPrefs.SetString(VERSION_KEY, jsonData);
                PlayerPrefs.Save();
                Debug.Log("[HotUpdate] 本地版本保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HotUpdate] 保存本地版本失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存本地更新清单
        /// </summary>
        private void SaveLocalManifest()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(_localManifest);
                PlayerPrefs.SetString(MANIFEST_KEY, jsonData);
                PlayerPrefs.Save();
                Debug.Log("[HotUpdate] 本地清单保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HotUpdate] 保存本地清单失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        public void CheckForUpdates()
        {
            if (!_isInitialized) return;

            Debug.Log("[HotUpdate] 检查游戏更新...");

            StartCoroutine(CheckForUpdatesCoroutine());
        }

        /// <summary>
        /// 检查更新协程
        /// </summary>
        private IEnumerator CheckForUpdatesCoroutine()
        {
            // 获取远程版本信息
            string versionUrl = UPDATE_SERVER + "version.json";
            using (UnityWebRequest request = UnityWebRequest.Get(versionUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    bool parseSuccess = false;
                    string errorMessage = "";
                    bool hasUpdate = false;
                    
                    try
                    {
                        string jsonData = request.downloadHandler.text;
                        _remoteVersion = JsonUtility.FromJson<VersionInfo>(jsonData);

                        Debug.Log($"[HotUpdate] 远程版本: {_remoteVersion.version}.{_remoteVersion.buildNumber}");

                        // 比较版本
                        hasUpdate = CompareVersions(_localVersion, _remoteVersion);
                        parseSuccess = true;
                        
                        if (!hasUpdate)
                        {
                            errorMessage = "当前已是最新版本";
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[HotUpdate] 解析版本信息失败: {ex.Message}");
                        parseSuccess = false;
                        errorMessage = "版本信息解析失败";
                    }
                    
                    if (parseSuccess && hasUpdate)
                    {
                        // 获取更新清单
                        yield return GetUpdateManifest();
                    }
                    else
                    {
                        OnVersionCheckCompleted?.Invoke(false, errorMessage);
                    }
                }
                else
                {
                    Debug.LogError($"[HotUpdate] 获取版本信息失败: {request.error}");
                    OnVersionCheckCompleted?.Invoke(false, "网络连接失败");
                }
            }
        }

        /// <summary>
        /// 比较版本
        /// </summary>
        private bool CompareVersions(VersionInfo local, VersionInfo remote)
        {
            if (remote.buildNumber > local.buildNumber)
            {
                return true;
            }
            else if (remote.buildNumber == local.buildNumber)
            {
                // 比较版本号
                string[] localParts = local.version.Split('.');
                string[] remoteParts = remote.version.Split('.');

                for (int i = 0; i < Math.Min(localParts.Length, remoteParts.Length); i++)
                {
                    if (int.TryParse(remoteParts[i], out int remoteNum) && 
                        int.TryParse(localParts[i], out int localNum))
                    {
                        if (remoteNum > localNum) return true;
                        if (remoteNum < localNum) return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取更新清单
        /// </summary>
        private IEnumerator GetUpdateManifest()
        {
            string manifestUrl = UPDATE_SERVER + $"manifest_{_remoteVersion.version}.json";
            using (UnityWebRequest request = UnityWebRequest.Get(manifestUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string jsonData = request.downloadHandler.text;
                        _remoteManifest = JsonUtility.FromJson<UpdateManifest>(jsonData);

                        Debug.Log($"[HotUpdate] 远程清单: {_remoteManifest.files.Count}个文件");

                        // 计算需要更新的文件
                        var filesToUpdate = GetFilesToUpdate();
                        if (filesToUpdate.Count > 0)
                        {
                            OnUpdateAvailable?.Invoke(_remoteManifest);
                            OnVersionCheckCompleted?.Invoke(true, $"发现{filesToUpdate.Count}个文件需要更新");
                        }
                        else
                        {
                            OnVersionCheckCompleted?.Invoke(false, "没有需要更新的文件");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[HotUpdate] 解析更新清单失败: {ex.Message}");
                        OnVersionCheckCompleted?.Invoke(false, "更新清单解析失败");
                    }
                }
                else
                {
                    Debug.LogError($"[HotUpdate] 获取更新清单失败: {request.error}");
                    OnVersionCheckCompleted?.Invoke(false, "获取更新清单失败");
                }
            }
        }

        /// <summary>
        /// 获取需要更新的文件列表
        /// </summary>
        private List<UpdateFile> GetFilesToUpdate()
        {
            var filesToUpdate = new List<UpdateFile>();

            foreach (var remoteFile in _remoteManifest.files)
            {
                bool needUpdate = true;

                // 检查本地是否有该文件
                var localFile = _localManifest.files.Find(f => f.filePath == remoteFile.filePath);
                if (localFile != null)
                {
                    // 比较文件哈希
                    if (localFile.fileHash == remoteFile.fileHash)
                    {
                        needUpdate = false;
                    }
                }

                if (needUpdate)
                {
                    filesToUpdate.Add(remoteFile);
                }
            }

            return filesToUpdate;
        }

        /// <summary>
        /// 开始下载更新
        /// </summary>
        public void StartDownload()
        {
            if (_isDownloading)
            {
                Debug.LogWarning("[HotUpdate] 正在下载中...");
                return;
            }

            if (_remoteManifest == null)
            {
                Debug.LogWarning("[HotUpdate] 没有可用的更新清单");
                return;
            }

            StartCoroutine(DownloadUpdateFiles());
        }

        /// <summary>
        /// 下载更新文件协程
        /// </summary>
        private IEnumerator DownloadUpdateFiles()
        {
            _isDownloading = true;
            _downloadProgress = 0f;

            var filesToUpdate = GetFilesToUpdate();
            int totalFiles = filesToUpdate.Count;
            int downloadedFiles = 0;

            Debug.Log($"[HotUpdate] 开始下载{totalFiles}个文件...");

            foreach (var file in filesToUpdate)
            {
                _currentDownloadFile = file.filePath;
                Debug.Log($"[HotUpdate] 下载文件: {file.filePath}");

                string fileUrl = UPDATE_SERVER + file.filePath;
                using (UnityWebRequest request = UnityWebRequest.Get(fileUrl))
                {
                    yield return request.SendWebRequest();

                    bool downloadSuccess = request.result == UnityWebRequest.Result.Success;
                    
                    if (downloadSuccess)
                    {
                        // 保存文件到本地
                        bool saveSuccess = SaveDownloadedFile(file.filePath, request.downloadHandler.data);
                        if (saveSuccess)
                        {
                            Debug.Log($"[HotUpdate] 文件下载成功: {file.filePath}");
                        }
                        else
                        {
                            Debug.LogError($"[HotUpdate] 文件保存失败: {file.filePath}");
                            OnDownloadCompleted?.Invoke(false, $"文件保存失败: {file.filePath}");
                            _isDownloading = false;
                            yield break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"[HotUpdate] 文件下载失败: {file.filePath} - {request.error}");
                        OnDownloadCompleted?.Invoke(false, $"文件下载失败: {file.filePath}");
                        _isDownloading = false;
                        yield break;
                    }
                }

                downloadedFiles++;
                _downloadProgress = (float)downloadedFiles / totalFiles;
                OnDownloadProgress?.Invoke(_downloadProgress);
            }

            // 更新本地版本信息
            _localVersion = _remoteVersion;
            _localManifest = _remoteManifest;
            SaveLocalVersion();
            SaveLocalManifest();

            _isDownloading = false;
            OnDownloadCompleted?.Invoke(true, "更新下载完成");
            Debug.Log("[HotUpdate] 所有文件下载完成");
        }

        /// <summary>
        /// 保存下载的文件
        /// </summary>
        private bool SaveDownloadedFile(string filePath, byte[] data)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(Application.persistentDataPath, filePath);
                string directory = System.IO.Path.GetDirectoryName(fullPath);
                
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                System.IO.File.WriteAllBytes(fullPath, data);
                Debug.Log($"[HotUpdate] 文件保存成功: {fullPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HotUpdate] 保存文件失败: {filePath} - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 应用更新
        /// </summary>
        public void ApplyUpdate()
        {
            if (_isDownloading)
            {
                Debug.LogWarning("[HotUpdate] 正在下载中，无法应用更新");
                return;
            }

            Debug.Log("[HotUpdate] 应用更新...");

            // 这里可以实现更新应用逻辑
            // 例如：重新加载资源、重启游戏等

            OnUpdateApplied?.Invoke("更新已应用");
            Debug.Log("[HotUpdate] 更新应用完成");
        }

        /// <summary>
        /// 获取下载进度
        /// </summary>
        public float GetDownloadProgress()
        {
            return _downloadProgress;
        }

        /// <summary>
        /// 获取当前下载文件
        /// </summary>
        public string GetCurrentDownloadFile()
        {
            return _currentDownloadFile;
        }

        /// <summary>
        /// 检查是否正在下载
        /// </summary>
        public bool IsDownloading()
        {
            return _isDownloading;
        }

        /// <summary>
        /// 获取版本信息
        /// </summary>
        public string GetVersionInfo()
        {
            return $"本地版本: {_localVersion.version}.{_localVersion.buildNumber}, " +
                   $"远程版本: {_remoteVersion?.version}.{_remoteVersion?.buildNumber ?? 0}";
        }

        /// <summary>
        /// 更新方法
        /// </summary>
        private void Update()
        {
            if (!_isInitialized) return;

            // 定时检查更新
            _checkTimer += Time.deltaTime;
            if (_checkTimer >= CHECK_INTERVAL)
            {
                _checkTimer = 0f;
                CheckForUpdates();
            }
        }

        /// <summary>
        /// 清除所有更新数据
        /// </summary>
        public void ClearAllData()
        {
            _localVersion = new VersionInfo
            {
                version = Application.version,
                buildNumber = 1,
                lastUpdate = DateTime.Now
            };
            _localManifest = new UpdateManifest
            {
                version = _localVersion.version,
                files = new List<UpdateFile>()
            };
            PlayerPrefs.DeleteKey(VERSION_KEY);
            PlayerPrefs.DeleteKey(MANIFEST_KEY);
            PlayerPrefs.Save();
            Debug.Log("[HotUpdate] 所有更新数据已清除");
        }
    }

    /// <summary>
    /// 版本信息
    /// </summary>
    [System.Serializable]
    public class VersionInfo
    {
        public string version;
        public int buildNumber;
        public DateTime lastUpdate;
    }

    /// <summary>
    /// 更新清单
    /// </summary>
    [System.Serializable]
    public class UpdateManifest
    {
        public string version;
        public List<UpdateFile> files;
    }

    /// <summary>
    /// 更新文件
    /// </summary>
    [System.Serializable]
    public class UpdateFile
    {
        public string filePath;
        public string fileHash;
        public long fileSize;
        public string description;
    }
}