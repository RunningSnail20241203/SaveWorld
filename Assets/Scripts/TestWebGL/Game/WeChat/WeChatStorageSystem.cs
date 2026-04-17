using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信存储系统
    /// 负责微信本地存储、云存储等功能
    /// </summary>
    public class WeChatStorageSystem : MonoBehaviour
    {
        private static WeChatStorageSystem s_instance;
        public static WeChatStorageSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("WeChatStorageSystem");
                    s_instance = go.AddComponent<WeChatStorageSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 存储配置
        private const string STORAGE_PREFIX = "wx_game_";
        private const int MAX_STORAGE_SIZE = 10 * 1024 * 1024; // 10MB限制
        
        // 初始化状态
        private bool _isInitialized = false;
        
        // 缓存数据
        private Dictionary<string, string> _localCache = new Dictionary<string, string>();

        // 事件
        public event Action<bool, string> OnStorageSaved;
        public event Action<bool, string, string> OnStorageLoaded;
        public event Action<bool, string> OnStorageRemoved;
        public event Action<bool, string> OnCloudStorageSaved;
        public event Action<bool, string, string> OnCloudStorageLoaded;

        /// <summary>
        /// 初始化微信存储系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[WeChatStorage] 初始化微信存储系统...");

            // 检查微信环境
            if (!WeChatAPI.IsAvailable())
            {
                Debug.LogWarning("[WeChatStorage] 微信环境不可用，存储功能受限");
            }

            // 加载本地缓存
            LoadLocalCache();

            _isInitialized = true;
            Debug.Log("[WeChatStorage] 微信存储系统初始化完成");
        }

        /// <summary>
        /// 保存数据到本地存储
        /// </summary>
        public void SaveLocal(string key, string data, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatStorage] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            string fullKey = STORAGE_PREFIX + key;
            
            Debug.Log($"[WeChatStorage] 保存本地数据：{key}");

            // 检查数据大小
            if (System.Text.Encoding.UTF8.GetByteCount(data) > MAX_STORAGE_SIZE)
            {
                Debug.LogError($"[WeChatStorage] 数据大小超过限制：{key}");
                callback?.Invoke(false, "数据大小超过限制");
                return;
            }

            // 调用微信存储API
            WeChatAPI.CallWeChatAPI("setStorage", $"{{\"key\":\"{fullKey}\",\"data\":\"{EscapeJsonString(data)}\"}}", (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                if (success)
                {
                    // 更新本地缓存
                    _localCache[key] = data;
                    Debug.Log($"[WeChatStorage] 本地数据保存成功：{key}");
                }
                else
                {
                    Debug.LogError($"[WeChatStorage] 本地数据保存失败：{key}");
                }

                OnStorageSaved?.Invoke(success, success ? "保存成功" : "保存失败");
                callback?.Invoke(success, success ? "保存成功" : "保存失败");
            });
        }

        /// <summary>
        /// 从本地存储加载数据
        /// </summary>
        public void LoadLocal(string key, Action<bool, string, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatStorage] 系统未初始化");
                callback?.Invoke(false, "系统未初始化", null);
                return;
            }

            string fullKey = STORAGE_PREFIX + key;
            
            Debug.Log($"[WeChatStorage] 加载本地数据：{key}");

            // 先检查缓存
            if (_localCache.ContainsKey(key))
            {
                Debug.Log($"[WeChatStorage] 从缓存加载数据：{key}");
                callback?.Invoke(true, "加载成功", _localCache[key]);
                return;
            }

            // 调用微信存储API
            WeChatAPI.CallWeChatAPI("getStorage", $"{{\"key\":\"{fullKey}\"}}", (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning($"[WeChatStorage] 本地数据不存在：{key}");
                    OnStorageLoaded?.Invoke(false, "数据不存在", null);
                    callback?.Invoke(false, "数据不存在", null);
                    return;
                }

                try
                {
                    var storageData = JsonUtility.FromJson<WeChatStorageResult>(result);
                    
                    if (storageData.success && !string.IsNullOrEmpty(storageData.data))
                    {
                        // 更新本地缓存
                        _localCache[key] = storageData.data;
                        
                        Debug.Log($"[WeChatStorage] 本地数据加载成功：{key}");
                        OnStorageLoaded?.Invoke(true, "加载成功", storageData.data);
                        callback?.Invoke(true, "加载成功", storageData.data);
                    }
                    else
                    {
                        Debug.LogWarning($"[WeChatStorage] 本地数据为空：{key}");
                        OnStorageLoaded?.Invoke(false, "数据为空", null);
                        callback?.Invoke(false, "数据为空", null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatStorage] 本地数据解析失败：{ex.Message}");
                    OnStorageLoaded?.Invoke(false, "数据解析失败", null);
                    callback?.Invoke(false, "数据解析失败", null);
                }
            });
        }

        /// <summary>
        /// 删除本地存储数据
        /// </summary>
        public void RemoveLocal(string key, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatStorage] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            string fullKey = STORAGE_PREFIX + key;
            
            Debug.Log($"[WeChatStorage] 删除本地数据：{key}");

            // 调用微信存储API
            WeChatAPI.CallWeChatAPI("removeStorage", $"{{\"key\":\"{fullKey}\"}}", (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                if (success)
                {
                    // 从缓存中移除
                    _localCache.Remove(key);
                    Debug.Log($"[WeChatStorage] 本地数据删除成功：{key}");
                }
                else
                {
                    Debug.LogError($"[WeChatStorage] 本地数据删除失败：{key}");
                }

                OnStorageRemoved?.Invoke(success, success ? "删除成功" : "删除失败");
                callback?.Invoke(success, success ? "删除成功" : "删除失败");
            });
        }

        /// <summary>
        /// 清空本地存储
        /// </summary>
        public void ClearLocal(Action<bool, string> callback = null)
        {
            Debug.Log("[WeChatStorage] 清空本地存储...");

            WeChatAPI.CallWeChatAPI("clearStorage", null, (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                if (success)
                {
                    _localCache.Clear();
                    Debug.Log("[WeChatStorage] 本地存储清空成功");
                }
                else
                {
                    Debug.LogError("[WeChatStorage] 本地存储清空失败");
                }

                callback?.Invoke(success, success ? "清空成功" : "清空失败");
            });
        }

        /// <summary>
        /// 保存数据到云存储
        /// </summary>
        public void SaveCloud(string key, string data, Action<bool, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatStorage] 系统未初始化");
                callback?.Invoke(false, "系统未初始化");
                return;
            }

            Debug.Log($"[WeChatStorage] 保存云数据：{key}");

            // 调用微信云存储API
            WeChatAPI.CallWeChatAPI("setUserCloudStorage", $"{{\"KVDataList\":[{{\"key\":\"{key}\",\"value\":\"{EscapeJsonString(data)}\"}}]}}", (result) =>
            {
                bool success = !string.IsNullOrEmpty(result) && result.Contains("\"success\":true");
                
                if (success)
                {
                    Debug.Log($"[WeChatStorage] 云数据保存成功：{key}");
                }
                else
                {
                    Debug.LogError($"[WeChatStorage] 云数据保存失败：{key}");
                }

                OnCloudStorageSaved?.Invoke(success, success ? "云保存成功" : "云保存失败");
                callback?.Invoke(success, success ? "云保存成功" : "云保存失败");
            });
        }

        /// <summary>
        /// 从云存储加载数据
        /// </summary>
        public void LoadCloud(string key, Action<bool, string, string> callback = null)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[WeChatStorage] 系统未初始化");
                callback?.Invoke(false, "系统未初始化", null);
                return;
            }

            Debug.Log($"[WeChatStorage] 加载云数据：{key}");

            // 调用微信云存储API
            WeChatAPI.CallWeChatAPI("getUserCloudStorage", $"{{\"keyList\":[\"{key}\"]}}", (result) =>
            {
                if (string.IsNullOrEmpty(result))
                {
                    Debug.LogWarning($"[WeChatStorage] 云数据不存在：{key}");
                    OnCloudStorageLoaded?.Invoke(false, "云数据不存在", null);
                    callback?.Invoke(false, "云数据不存在", null);
                    return;
                }

                try
                {
                    var cloudData = JsonUtility.FromJson<WeChatCloudStorageResult>(result);
                    
                    if (cloudData.success && cloudData.KVDataList != null && cloudData.KVDataList.Count > 0)
                    {
                        string data = cloudData.KVDataList[0].value;
                        
                        Debug.Log($"[WeChatStorage] 云数据加载成功：{key}");
                        OnCloudStorageLoaded?.Invoke(true, "云加载成功", data);
                        callback?.Invoke(true, "云加载成功", data);
                    }
                    else
                    {
                        Debug.LogWarning($"[WeChatStorage] 云数据为空：{key}");
                        OnCloudStorageLoaded?.Invoke(false, "云数据为空", null);
                        callback?.Invoke(false, "云数据为空", null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WeChatStorage] 云数据解析失败：{ex.Message}");
                    OnCloudStorageLoaded?.Invoke(false, "云数据解析失败", null);
                    callback?.Invoke(false, "云数据解析失败", null);
                }
            });
        }

        /// <summary>
        /// 加载本地缓存
        /// </summary>
        private void LoadLocalCache()
        {
            // 这里可以实现从PlayerPrefs或其他本地存储加载缓存
            Debug.Log("[WeChatStorage] 加载本地缓存...");
        }

        /// <summary>
        /// 转义JSON字符串
        /// </summary>
        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            
            return str.Replace("\\", "\\\\")
                     .Replace("\"", "\\\"")
                     .Replace("\n", "\\n")
                     .Replace("\r", "\\r")
                     .Replace("\t", "\\t");
        }

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        public bool HasKey(string key)
        {
            return _localCache.ContainsKey(key);
        }

        /// <summary>
        /// 获取所有键
        /// </summary>
        public List<string> GetAllKeys()
        {
            return new List<string>(_localCache.Keys);
        }

        /// <summary>
        /// 获取存储统计信息
        /// </summary>
        public string GetStorageInfo()
        {
            return $"本地缓存：{_localCache.Count}项, " +
                   $"微信环境：{(WeChatAPI.IsAvailable() ? "可用" : "不可用")}";
        }
    }

    /// <summary>
    /// 微信存储结果
    /// </summary>
    [System.Serializable]
    public class WeChatStorageResult
    {
        public bool success;
        public string data;
        public string error;
    }

    /// <summary>
    /// 微信云存储结果
    /// </summary>
    [System.Serializable]
    public class WeChatCloudStorageResult
    {
        public bool success;
        public List<WeChatKVData> KVDataList;
        public string error;
    }

    /// <summary>
    /// 微信键值对数据
    /// </summary>
    [System.Serializable]
    public class WeChatKVData
    {
        public string key;
        public string value;
    }
}