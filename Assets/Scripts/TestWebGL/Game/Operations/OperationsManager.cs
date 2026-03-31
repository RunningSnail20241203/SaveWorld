using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestWebGL.Game.Operations
{
    /// <summary>
    /// 运营管理器
    /// 负责游戏运营相关的功能管理
    /// </summary>
    public class OperationsManager : MonoBehaviour
    {
        private static OperationsManager s_instance;
        public static OperationsManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("OperationsManager");
                    s_instance = go.AddComponent<OperationsManager>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 运营配置
        private const string OPERATIONS_KEY = "operations_data";
        private const float UPDATE_CHECK_INTERVAL = 3600f; // 1小时检查一次

        // 运营数据
        private OperationsData _operationsData;
        
        // 更新检查计时器
        private float _updateCheckTimer = 0f;
        
        // 初始化状态
        private bool _isInitialized = false;

        // 事件
        public event Action<bool, string> OnUpdateCheckCompleted;
        public event Action<AnnouncementData> OnAnnouncementReceived;
        public event Action<MaintenanceData> OnMaintenanceNotice;
        public event Action<OperationsEventData> OnEventStarted;
        public event Action<OperationsEventData> OnEventEnded;

        /// <summary>
        /// 初始化运营管理器
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[Operations] 初始化运营管理器...");

            // 加载运营数据
            LoadOperationsData();

            // 检查更新
            CheckForUpdates();

            // 检查公告
            CheckAnnouncements();

            // 检查维护通知
            CheckMaintenanceNotice();

            // 检查活动
            CheckEvents();

            _isInitialized = true;
            Debug.Log("[Operations] 运营管理器初始化完成");
        }

        /// <summary>
        /// 加载运营数据
        /// </summary>
        private void LoadOperationsData()
        {
            try
            {
                if (PlayerPrefs.HasKey(OPERATIONS_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(OPERATIONS_KEY);
                    _operationsData = JsonUtility.FromJson<OperationsData>(jsonData);
                }

                if (_operationsData == null)
                {
                    _operationsData = new OperationsData
                    {
                        lastUpdateCheck = DateTime.MinValue,
                        lastAnnouncementId = "",
                        activeEvents = new List<string>(),
                        maintenanceMode = false
                    };
                }

                Debug.Log("[Operations] 运营数据加载成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Operations] 加载运营数据失败: {ex.Message}");
                _operationsData = new OperationsData
                {
                    lastUpdateCheck = DateTime.MinValue,
                    lastAnnouncementId = "",
                    activeEvents = new List<string>(),
                    maintenanceMode = false
                };
            }
        }

        /// <summary>
        /// 保存运营数据
        /// </summary>
        private void SaveOperationsData()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(_operationsData);
                PlayerPrefs.SetString(OPERATIONS_KEY, jsonData);
                PlayerPrefs.Save();
                Debug.Log("[Operations] 运营数据保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Operations] 保存运营数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        public void CheckForUpdates()
        {
            Debug.Log("[Operations] 检查游戏更新...");

            // 这里实现实际的更新检查逻辑
            // 可以调用微信API或自建服务器

            // 模拟更新检查
            bool hasUpdate = false;
            string updateInfo = "";

            _operationsData.lastUpdateCheck = DateTime.Now;
            SaveOperationsData();

            OnUpdateCheckCompleted?.Invoke(hasUpdate, updateInfo);

            Debug.Log($"[Operations] 更新检查完成: {(hasUpdate ? "有更新" : "无更新")}");
        }

        /// <summary>
        /// 检查公告
        /// </summary>
        public void CheckAnnouncements()
        {
            Debug.Log("[Operations] 检查游戏公告...");

            // 这里实现实际的公告检查逻辑
            // 可以调用微信API或自建服务器

            // 模拟公告检查
            var announcement = new AnnouncementData
            {
                id = "announcement_001",
                title = "欢迎来到末世生存合成",
                content = "感谢您体验我们的游戏！如有任何问题，请联系客服。",
                publishTime = DateTime.Now,
                isRead = false
            };

            if (announcement.id != _operationsData.lastAnnouncementId)
            {
                _operationsData.lastAnnouncementId = announcement.id;
                SaveOperationsData();
                OnAnnouncementReceived?.Invoke(announcement);
            }

            Debug.Log("[Operations] 公告检查完成");
        }

        /// <summary>
        /// 检查维护通知
        /// </summary>
        public void CheckMaintenanceNotice()
        {
            Debug.Log("[Operations] 检查维护通知...");

            // 这里实现实际的维护通知检查逻辑

            // 模拟维护通知检查
            var maintenance = new MaintenanceData
            {
                isMaintenance = false,
                startTime = DateTime.MinValue,
                endTime = DateTime.MinValue,
                reason = ""
            };

            if (maintenance.isMaintenance)
            {
                _operationsData.maintenanceMode = true;
                SaveOperationsData();
                OnMaintenanceNotice?.Invoke(maintenance);
            }

            Debug.Log("[Operations] 维护通知检查完成");
        }

        /// <summary>
        /// 检查活动
        /// </summary>
        public void CheckEvents()
        {
            Debug.Log("[Operations] 检查游戏活动...");

            // 这里实现实际的活动检查逻辑

            // 模拟活动检查
            var activeEvent = new OperationsEventData
            {
                eventId = "event_001",
                eventName = "新手福利活动",
                description = "新玩家登录即可获得丰厚奖励！",
                startTime = DateTime.Now.AddDays(-1),
                endTime = DateTime.Now.AddDays(7),
                isActive = true,
                rewards = new List<string> { "能量饮料×5", "探索加速券×3" }
            };

            if (activeEvent.isActive && !_operationsData.activeEvents.Contains(activeEvent.eventId))
            {
                _operationsData.activeEvents.Add(activeEvent.eventId);
                SaveOperationsData();
                OnEventStarted?.Invoke(activeEvent);
            }

            Debug.Log("[Operations] 活动检查完成");
        }

        /// <summary>
        /// 获取游戏版本
        /// </summary>
        public string GetGameVersion()
        {
            return Application.version;
        }

        /// <summary>
        /// 获取构建版本
        /// </summary>
        public string GetBuildVersion()
        {
            return $"{Application.version}.{Application.buildGUID}";
        }

        /// <summary>
        /// 获取平台信息
        /// </summary>
        public string GetPlatformInfo()
        {
            return $"平台: {Application.platform}, 版本: {Application.version}";
        }

        /// <summary>
        /// 获取运营统计信息
        /// </summary>
        public string GetOperationsInfo()
        {
            return $"版本: {GetGameVersion()}, " +
                   $"最后更新检查: {_operationsData.lastUpdateCheck:yyyy-MM-dd HH:mm:ss}, " +
                   $"活跃活动: {_operationsData.activeEvents.Count}, " +
                   $"维护模式: {(_operationsData.maintenanceMode ? "是" : "否")}";
        }

        /// <summary>
        /// 检查是否处于维护模式
        /// </summary>
        public bool IsMaintenanceMode()
        {
            return _operationsData.maintenanceMode;
        }

        /// <summary>
        /// 设置维护模式
        /// </summary>
        public void SetMaintenanceMode(bool isMaintenance)
        {
            _operationsData.maintenanceMode = isMaintenance;
            SaveOperationsData();
            Debug.Log($"[Operations] 维护模式设置为: {isMaintenance}");
        }

        /// <summary>
        /// 获取活跃活动列表
        /// </summary>
        public List<string> GetActiveEvents()
        {
            return new List<string>(_operationsData.activeEvents);
        }

        /// <summary>
        /// 添加活跃活动
        /// </summary>
        public void AddActiveEvent(string eventId)
        {
            if (!_operationsData.activeEvents.Contains(eventId))
            {
                _operationsData.activeEvents.Add(eventId);
                SaveOperationsData();
                Debug.Log($"[Operations] 添加活跃活动: {eventId}");
            }
        }

        /// <summary>
        /// 移除活跃活动
        /// </summary>
        public void RemoveActiveEvent(string eventId)
        {
            if (_operationsData.activeEvents.Contains(eventId))
            {
                _operationsData.activeEvents.Remove(eventId);
                SaveOperationsData();
                Debug.Log($"[Operations] 移除活跃活动: {eventId}");
            }
        }

        /// <summary>
        /// 更新方法
        /// </summary>
        private void Update()
        {
            if (!_isInitialized) return;

            // 定时检查更新
            _updateCheckTimer += Time.deltaTime;
            if (_updateCheckTimer >= UPDATE_CHECK_INTERVAL)
            {
                _updateCheckTimer = 0f;
                CheckForUpdates();
            }
        }

        /// <summary>
        /// 清除所有运营数据
        /// </summary>
        public void ClearAllData()
        {
            _operationsData = new OperationsData
            {
                lastUpdateCheck = DateTime.MinValue,
                lastAnnouncementId = "",
                activeEvents = new List<string>(),
                maintenanceMode = false
            };
            PlayerPrefs.DeleteKey(OPERATIONS_KEY);
            PlayerPrefs.Save();
            Debug.Log("[Operations] 所有运营数据已清除");
        }
    }

    /// <summary>
    /// 运营数据
    /// </summary>
    [System.Serializable]
    public class OperationsData
    {
        public DateTime lastUpdateCheck;
        public string lastAnnouncementId;
        public List<string> activeEvents;
        public bool maintenanceMode;
    }

    /// <summary>
    /// 公告数据
    /// </summary>
    [System.Serializable]
    public class AnnouncementData
    {
        public string id;
        public string title;
        public string content;
        public DateTime publishTime;
        public bool isRead;
    }

    /// <summary>
    /// 维护数据
    /// </summary>
    [System.Serializable]
    public class MaintenanceData
    {
        public bool isMaintenance;
        public DateTime startTime;
        public DateTime endTime;
        public string reason;
    }

    /// <summary>
    /// 运营活动数据
    /// </summary>
    [System.Serializable]
    public class OperationsEventData
    {
        public string eventId;
        public string eventName;
        public string description;
        public DateTime startTime;
        public DateTime endTime;
        public bool isActive;
        public List<string> rewards;
    }
}