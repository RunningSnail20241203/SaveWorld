using System;
using UnityEngine;
using WeChatWASM;

namespace SaveWorld.Game.WeChat
{
    /// <summary>
    /// 微信存储系统 - 本地存储 + 云存储
    /// 基于 WX.StorageSetStringSync / WX.SetUserCloudStorage
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

        private string _storagePrefix = "wx_game_";

        public event Action<bool, string> OnStorageSaved;
        public event Action<bool, string, string> OnStorageLoaded;
        public event Action<bool, string> OnStorageRemoved;
        public event Action<bool, string> OnCloudStorageSaved;

        /// <summary>
        /// 从 WeChatConfigManager 加载存储配置
        /// </summary>
        public void LoadConfig()
        {
            _storagePrefix = WeChatConfigManager.SaveDataKeyPrefix;
            if (string.IsNullOrEmpty(_storagePrefix))
            {
                _storagePrefix = "wx_game_";
            }
            Debug.Log($"[WeChatStorage] 配置加载完成: prefix={_storagePrefix}");
        }

        /// <summary>
        /// 同步保存字符串到本地存储
        /// </summary>
        public void SaveLocalSync(string key, string data)
        {
            try
            {
                string fullKey = _storagePrefix + key;
                WX.StorageSetStringSync(fullKey, data);
                Debug.Log($"[WeChatStorage] 本地保存成功: {key}");
                OnStorageSaved?.Invoke(true, "保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WeChatStorage] 本地保存失败: {key} - {ex.Message}");
                OnStorageSaved?.Invoke(false, ex.Message);
            }
        }

        /// <summary>
        /// 同步从本地存储加载字符串
        /// </summary>
        public string LoadLocalSync(string key, string defaultValue = "")
        {
            try
            {
                string fullKey = _storagePrefix + key;
                if (WX.StorageHasKeySync(fullKey))
                {
                    string data = WX.StorageGetStringSync(fullKey, defaultValue);
                    OnStorageLoaded?.Invoke(true, "加载成功", data);
                    return data;
                }
                else
                {
                    OnStorageLoaded?.Invoke(false, "键不存在", null);
                    return defaultValue;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WeChatStorage] 本地加载失败: {key} - {ex.Message}");
                OnStorageLoaded?.Invoke(false, ex.Message, null);
                return defaultValue;
            }
        }

        /// <summary>
        /// 同步保存 int
        /// </summary>
        public void SaveIntSync(string key, int value)
        {
            string fullKey = _storagePrefix + key;
            WX.StorageSetIntSync(fullKey, value);
        }

        /// <summary>
        /// 同步读取 int
        /// </summary>
        public int LoadIntSync(string key, int defaultValue = 0)
        {
            string fullKey = _storagePrefix + key;
            return WX.StorageGetIntSync(fullKey, defaultValue);
        }

        /// <summary>
        /// 删除本地存储
        /// </summary>
        public void RemoveLocalSync(string key)
        {
            try
            {
                string fullKey = _storagePrefix + key;
                WX.StorageDeleteKeySync(fullKey);
                Debug.Log($"[WeChatStorage] 本地删除成功: {key}");
                OnStorageRemoved?.Invoke(true, "删除成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WeChatStorage] 本地删除失败: {key} - {ex.Message}");
                OnStorageRemoved?.Invoke(false, ex.Message);
            }
        }

        /// <summary>
        /// 清空所有本地存储
        /// </summary>
        public void ClearAllSync()
        {
            WX.StorageDeleteAllSync();
            Debug.Log("[WeChatStorage] 本地存储已清空");
        }

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        public bool HasKey(string key)
        {
            string fullKey = _storagePrefix + key;
            return WX.StorageHasKeySync(fullKey);
        }

        /// <summary>
        /// 保存数据到微信云存储（用于排行榜等社交功能）
        /// 注意：云存储读取需要通过开放数据域实现
        /// </summary>
        public void SaveCloud(string key, string data, Action<bool, string> callback = null)
        {
            Debug.Log($"[WeChatStorage] 保存云数据: {key}");

            WX.SetUserCloudStorage(new SetUserCloudStorageOption
            {
                KVDataList = new KVData[]
                {
                    new KVData { key = key, value = data }
                },
                success = (res) =>
                {
                    Debug.Log($"[WeChatStorage] 云数据保存成功: {key}");
                    OnCloudStorageSaved?.Invoke(true, "云保存成功");
                    callback?.Invoke(true, "云保存成功");
                },
                fail = (res) =>
                {
                    Debug.LogError($"[WeChatStorage] 云数据保存失败: {key} - {res.errMsg}");
                    OnCloudStorageSaved?.Invoke(false, res.errMsg);
                    callback?.Invoke(false, res.errMsg);
                }
            });
        }

        public string GetStorageInfo()
        {
            return "微信存储系统就绪";
        }
    }
}
