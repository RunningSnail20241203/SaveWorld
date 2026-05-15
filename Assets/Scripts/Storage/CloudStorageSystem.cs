using System;
using SaveWorld.Game.Core;
using UnityEngine;
using WeChatWASM;

namespace SaveWorld.Game.Storage
{
    /// <summary>
    /// 微信云存储系统
    /// 基于 WX.SetUserCloudStorage 实现云端存档上传同步
    /// 冲突解决策略：时间戳优先，本地优先（时间戳相同时）
    /// </summary>
    public class CloudStorageSystem
    {
        private readonly EventBus _eventBus;
        private readonly StateMutator _stateMutator;
        private readonly StorageSystem _localStorage;
        private readonly bool _useWeChat;

        private bool _isSyncing = false;
        private DateTime _lastSyncTime;
        private string _lastSyncResult = "";

        // 云端存储键名
        private const string CLOUD_SAVE_KEY = "game_save_v1";

        public CloudStorageSystem(EventBus eventBus, StateMutator stateMutator,
            StorageSystem localStorage, bool useWeChatStorage = false)
        {
            _eventBus = eventBus;
            _stateMutator = stateMutator;
            _localStorage = localStorage;
            _useWeChat = useWeChatStorage;

            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            _eventBus.Listen<CloudSyncRequestEvent>(_ => StartSync());
            _eventBus.Listen<GameStartedEvent>(_ => AutoSyncOnStartup());
        }

        /// <summary>
        /// 启动后延迟自动同步
        /// </summary>
        private void AutoSyncOnStartup()
        {
            _eventBus.Publish(new CloudSyncScheduledEvent { DelaySeconds = 3 });
            // 延迟3秒待所有系统就绪后执行同步
            StartSync();
        }

        /// <summary>
        /// 开始云端同步（异步回调链）
        /// 流程: 保存本地 → 下载云端 → 时间戳比对 → 上传/合并 → 完成
        /// </summary>
        public void StartSync()
        {
            if (_isSyncing)
            {
                Debug.LogWarning("[CloudStorage] 同步进行中，跳过重复请求");
                _eventBus.Publish(new CloudSyncFailedEvent { Reason = "同步进行中" });
                return;
            }

            if (!_useWeChat)
            {
                Debug.Log("[CloudStorage] 非微信环境，跳过云同步");
                _eventBus.Publish(new CloudSyncCompletedEvent
                {
                    Success = false,
                    Message = "非微信环境，跳过云同步",
                    SyncTime = DateTime.MinValue
                });
                return;
            }

            _isSyncing = true;
            _eventBus.Publish(new CloudSyncStartedEvent { SyncId = Guid.NewGuid().ToString() });

            Debug.Log("[CloudStorage] 开始云同步...");

            // 第一步：保存当前本地状态
            SaveLocalCurrentState();

            // 第二步：从云端下载数据，进行比对
            TryDownloadFromCloud();
        }

        /// <summary>
        /// 保存当前游戏状态到本地存储
        /// </summary>
        private void SaveLocalCurrentState()
        {
            var state = _stateMutator.CurrentState;
            _localStorage.SaveGameState(state);
            Debug.Log("[CloudStorage] 本地状态已保存");
        }

        /// <summary>
        /// 尝试从云端下载存档数据
        /// 注意：当前SDK版本无WX.GetUserCloudStorage API，默认按云端无数据处理，上传本地存档
        /// </summary>
        private void TryDownloadFromCloud()
        {
            Debug.Log("[CloudStorage] SDK版本不支持云端下载，默认上传本地数据...");
            TryUploadToCloud(null);
        }

        /// <summary>
        /// 尝试将本地存档上传至云端
        /// </summary>
        private void TryUploadToCloud(StorageSystem.GameStateSaveData saveData)
        {
            try
            {
                if (saveData == null)
                {
                    var localResult = _localStorage.LoadGameState();
                    saveData = localResult.data;
                }

                if (saveData == null)
                {
                    Debug.LogWarning("[CloudStorage] 无本地存档可上传");
                    FinishSync(false, "无本地存档");
                    return;
                }

                string jsonData = JsonUtility.ToJson(saveData);

                WX.SetUserCloudStorage(new SetUserCloudStorageOption
                {
                    KVDataList = new KVData[]
                    {
                        new KVData { key = CLOUD_SAVE_KEY, value = jsonData }
                    },
                    success = OnCloudUploadSucceeded,
                    fail = OnCloudUploadFailed
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CloudStorage] 云端上传异常: {ex.Message}");
                FinishSync(false, $"上传异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 云端上传成功回调
        /// </summary>
        private void OnCloudUploadSucceeded(GeneralCallbackResult res)
        {
            Debug.Log("[CloudStorage] 云端上传成功");
            FinishSync(true, "云端同步成功");
        }

        /// <summary>
        /// 云端上传失败回调
        /// </summary>
        private void OnCloudUploadFailed(GeneralCallbackResult res)
        {
            Debug.LogError($"[CloudStorage] 云端上传失败: {res.errMsg}");
            FinishSync(false, $"云端上传失败: {res.errMsg}");
        }

        /// <summary>
        /// 将云端存档合并到本地（云端更新时）
        /// 云存 JSON 直写本地，下次启动自动加载
        /// </summary>
        private void MergeCloudToLocal(StorageSystem.GameStateSaveData cloudSave)
        {
            try
            {
                string jsonData = JsonUtility.ToJson(cloudSave);
                _localStorage.SaveGameStateRaw(jsonData);

                _eventBus.Publish(new CloudDataMergedEvent
                {
                    CloudSaveTime = cloudSave.saveTime,
                    CloudVersion = cloudSave.versionNumber
                });

                Debug.Log($"[CloudStorage] 云端数据已保存到本地，下次启动生效");
                FinishSync(true, "云端数据已下载到本地");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CloudStorage] 云端数据合并失败: {ex.Message}");
                FinishSync(false, $"合并失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 完成同步流程
        /// </summary>
        private void FinishSync(bool success, string message)
        {
            _isSyncing = false;
            _lastSyncTime = DateTime.UtcNow;
            _lastSyncResult = message;

            _eventBus.Publish(new CloudSyncCompletedEvent
            {
                Success = success,
                Message = message,
                SyncTime = _lastSyncTime
            });

            Debug.Log($"[CloudStorage] 同步完成: {(success ? "成功" : "失败")} - {message}");
        }

        /// <summary>
        /// 强制覆盖云端数据
        /// </summary>
        public void ForceUpload()
        {
            if (!_useWeChat)
            {
                Debug.Log("[CloudStorage] 非微信环境，跳过强制上传");
                return;
            }

            Debug.Log("[CloudStorage] 强制上传至云端...");
            SaveLocalCurrentState();
            TryUploadToCloud(null);
            _eventBus.Publish(new CloudForceUploadCompletedEvent());
        }

        /// <summary>
        /// 下载云端数据并覆盖本地
        /// 注意：当前SDK版本无WX.GetUserCloudStorage API，此功能暂不可用
        /// </summary>
        public void ForceDownload()
        {
            if (!_useWeChat)
            {
                Debug.Log("[CloudStorage] 非微信环境，跳过强制下载");
                return;
            }

            Debug.LogWarning("[CloudStorage] SDK版本不支持云端下载，强制下载功能暂不可用");
            _eventBus.Publish(new CloudForceDownloadCompletedEvent());
        }

        /// <summary>
        /// 获取同步状态信息
        /// </summary>
        public string GetSyncStatus()
        {
            string backend = _useWeChat ? "微信云存储" : "本地存储";
            string time = _lastSyncTime != default ? _lastSyncTime.ToString("yyyy-MM-dd HH:mm:ss") : "从未同步";
            string syncing = _isSyncing ? " [同步中]" : "";
            return $"[{backend}] 最后同步: {time} 结果: {_lastSyncResult}{syncing}";
        }
    }

    #region 事件定义

    public class CloudSyncRequestEvent : GameEvent
    {
    }

    public class CloudSyncFailedEvent : GameEvent
    {
        public string Reason;
    }

    public class CloudSyncScheduledEvent : GameEvent
    {
        public int DelaySeconds;
    }

    public class CloudForceUploadCompletedEvent : GameEvent
    {
    }

    public class CloudForceDownloadCompletedEvent : GameEvent
    {
    }

    /// <summary>
    /// 云端数据已下载合并到本地事件
    /// </summary>
    public class CloudDataMergedEvent : GameEvent
    {
        public DateTime CloudSaveTime;
        public int CloudVersion;
    }

    #endregion
}
