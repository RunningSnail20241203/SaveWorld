using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestWebGL.Game.Analytics
{
    /// <summary>
    /// 数据分析系统
    /// 负责收集玩家行为数据，优化游戏体验
    /// </summary>
    public class AnalyticsSystem : MonoBehaviour
    {
        private static AnalyticsSystem s_instance;
        public static AnalyticsSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("AnalyticsSystem");
                    s_instance = go.AddComponent<AnalyticsSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 分析配置
        private const string ANALYTICS_KEY = "analytics_data";
        private const int MAX_EVENT_QUEUE_SIZE = 100;
        private const float UPLOAD_INTERVAL = 60f; // 60秒上传一次

        // 事件队列
        private Queue<AnalyticsEvent> _eventQueue = new Queue<AnalyticsEvent>();
        
        // 玩家会话数据
        private SessionData _currentSession;
        
        // 上传计时器
        private float _uploadTimer = 0f;
        
        // 初始化状态
        private bool _isInitialized = false;

        /// <summary>
        /// 初始化数据分析系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[Analytics] 初始化数据分析系统...");

            // 开始新会话
            StartNewSession();

            // 加载本地数据
            LoadLocalData();

            _isInitialized = true;
            Debug.Log("[Analytics] 数据分析系统初始化完成");
        }

        /// <summary>
        /// 开始新会话
        /// </summary>
        private void StartNewSession()
        {
            _currentSession = new SessionData
            {
                sessionId = Guid.NewGuid().ToString(),
                startTime = DateTime.Now,
                deviceInfo = GetDeviceInfo(),
                events = new List<AnalyticsEvent>()
            };

            Debug.Log($"[Analytics] 开始新会话: {_currentSession.sessionId}");
        }

        /// <summary>
        /// 获取设备信息
        /// </summary>
        private DeviceInfo GetDeviceInfo()
        {
            return new DeviceInfo
            {
                platform = Application.platform.ToString(),
                deviceModel = SystemInfo.deviceModel,
                operatingSystem = SystemInfo.operatingSystem,
                processorType = SystemInfo.processorType,
                systemMemorySize = SystemInfo.systemMemorySize,
                graphicsMemorySize = SystemInfo.graphicsMemorySize,
                screenWidth = Screen.width,
                screenHeight = Screen.height,
                dpi = Screen.dpi
            };
        }

        /// <summary>
        /// 记录事件
        /// </summary>
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_isInitialized) return;

            var analyticsEvent = new AnalyticsEvent
            {
                eventName = eventName,
                timestamp = DateTime.Now,
                parameters = parameters ?? new Dictionary<string, object>()
            };

            _eventQueue.Enqueue(analyticsEvent);
            _currentSession.events.Add(analyticsEvent);

            Debug.Log($"[Analytics] 记录事件: {eventName}");

            // 检查队列大小
            if (_eventQueue.Count >= MAX_EVENT_QUEUE_SIZE)
            {
                UploadEvents();
            }
        }

        /// <summary>
        /// 记录玩家登录
        /// </summary>
        public void LogPlayerLogin(string playerId)
        {
            LogEvent("player_login", new Dictionary<string, object>
            {
                { "player_id", playerId },
                { "login_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            });
        }

        /// <summary>
        /// 记录玩家升级
        /// </summary>
        public void LogPlayerLevelUp(int newLevel, int oldLevel)
        {
            LogEvent("player_level_up", new Dictionary<string, object>
            {
                { "new_level", newLevel },
                { "old_level", oldLevel },
                { "level_diff", newLevel - oldLevel }
            });
        }

        /// <summary>
        /// 记录合成事件
        /// </summary>
        public void LogCraftItem(string itemType, int itemCount, bool success)
        {
            LogEvent("craft_item", new Dictionary<string, object>
            {
                { "item_type", itemType },
                { "item_count", itemCount },
                { "success", success }
            });
        }

        /// <summary>
        /// 记录探索事件
        /// </summary>
        public void LogExplore(int staminaCost, string[] itemsObtained)
        {
            LogEvent("explore", new Dictionary<string, object>
            {
                { "stamina_cost", staminaCost },
                { "items_obtained", itemsObtained },
                { "item_count", itemsObtained.Length }
            });
        }

        /// <summary>
        /// 记录订单完成
        /// </summary>
        public void LogOrderComplete(string orderType, int orderLevel, string rewardType)
        {
            LogEvent("order_complete", new Dictionary<string, object>
            {
                { "order_type", orderType },
                { "order_level", orderLevel },
                { "reward_type", rewardType }
            });
        }

        /// <summary>
        /// 记录成就解锁
        /// </summary>
        public void LogAchievementUnlock(string achievementType)
        {
            LogEvent("achievement_unlock", new Dictionary<string, object>
            {
                { "achievement_type", achievementType }
            });
        }

        /// <summary>
        /// 记录游戏时长
        /// </summary>
        public void LogGameSession(float sessionDuration)
        {
            LogEvent("game_session", new Dictionary<string, object>
            {
                { "session_duration", sessionDuration },
                { "session_id", _currentSession.sessionId }
            });
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        public void LogError(string errorType, string errorMessage, string stackTrace = null)
        {
            LogEvent("error", new Dictionary<string, object>
            {
                { "error_type", errorType },
                { "error_message", errorMessage },
                { "stack_trace", stackTrace ?? "" }
            });
        }

        /// <summary>
        /// 记录性能指标
        /// </summary>
        public void LogPerformance(string metricName, float value)
        {
            LogEvent("performance", new Dictionary<string, object>
            {
                { "metric_name", metricName },
                { "value", value }
            });
        }

        /// <summary>
        /// 上传事件到服务器
        /// </summary>
        private void UploadEvents()
        {
            if (_eventQueue.Count == 0) return;

            Debug.Log($"[Analytics] 上传 {_eventQueue.Count} 个事件...");

            // 这里实现实际的上传逻辑
            // 可以调用微信云开发API或自建服务器
            
            // 清空队列
            _eventQueue.Clear();
            
            // 保存本地数据
            SaveLocalData();
        }

        /// <summary>
        /// 保存本地数据
        /// </summary>
        private void SaveLocalData()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(_currentSession);
                PlayerPrefs.SetString(ANALYTICS_KEY, jsonData);
                PlayerPrefs.Save();
                Debug.Log("[Analytics] 本地数据保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Analytics] 保存本地数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载本地数据
        /// </summary>
        private void LoadLocalData()
        {
            try
            {
                if (PlayerPrefs.HasKey(ANALYTICS_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(ANALYTICS_KEY);
                    // 这里可以加载历史数据进行分析
                    Debug.Log("[Analytics] 本地数据加载成功");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Analytics] 加载本地数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新方法
        /// </summary>
        private void Update()
        {
            if (!_isInitialized) return;

            // 定时上传
            _uploadTimer += Time.deltaTime;
            if (_uploadTimer >= UPLOAD_INTERVAL)
            {
                _uploadTimer = 0f;
                UploadEvents();
            }
        }

        /// <summary>
        /// 应用退出时保存数据
        /// </summary>
        private void OnApplicationQuit()
        {
            if (_currentSession != null)
            {
                _currentSession.endTime = DateTime.Now;
                _currentSession.sessionDuration = (float)(_currentSession.endTime - _currentSession.startTime).TotalSeconds;
                
                LogGameSession(_currentSession.sessionDuration);
                UploadEvents();
            }
        }

        /// <summary>
        /// 获取分析统计信息
        /// </summary>
        public string GetAnalyticsInfo()
        {
            return $"会话ID: {_currentSession?.sessionId ?? "无"}, " +
                   $"事件队列: {_eventQueue.Count}, " +
                   $"会话时长: {_currentSession?.sessionDuration ?? 0:F1}秒";
        }

        /// <summary>
        /// 获取玩家留存统计
        /// </summary>
        public void GetRetentionStats(Action<RetentionData> callback)
        {
            // 这里实现留存统计逻辑
            var retentionData = new RetentionData
            {
                day1Retention = 0.8f,
                day7Retention = 0.5f,
                day30Retention = 0.2f
            };
            
            callback?.Invoke(retentionData);
        }

        /// <summary>
        /// 获取功能使用频率
        /// </summary>
        public void GetFeatureUsageStats(Action<FeatureUsageData> callback)
        {
            // 这里实现功能使用统计逻辑
            var featureUsage = new FeatureUsageData
            {
                exploreUsage = 100,
                craftUsage = 80,
                orderUsage = 60,
                achievementUsage = 40
            };
            
            callback?.Invoke(featureUsage);
        }
    }

    /// <summary>
    /// 分析事件
    /// </summary>
    [System.Serializable]
    public class AnalyticsEvent
    {
        public string eventName;
        public DateTime timestamp;
        public Dictionary<string, object> parameters;
    }

    /// <summary>
    /// 会话数据
    /// </summary>
    [System.Serializable]
    public class SessionData
    {
        public string sessionId;
        public DateTime startTime;
        public DateTime endTime;
        public float sessionDuration;
        public DeviceInfo deviceInfo;
        public List<AnalyticsEvent> events;
    }

    /// <summary>
    /// 设备信息
    /// </summary>
    [System.Serializable]
    public class DeviceInfo
    {
        public string platform;
        public string deviceModel;
        public string operatingSystem;
        public string processorType;
        public int systemMemorySize;
        public int graphicsMemorySize;
        public int screenWidth;
        public int screenHeight;
        public float dpi;
    }

    /// <summary>
    /// 留存数据
    /// </summary>
    [System.Serializable]
    public class RetentionData
    {
        public float day1Retention;
        public float day7Retention;
        public float day30Retention;
    }

    /// <summary>
    /// 功能使用数据
    /// </summary>
    [System.Serializable]
    public class FeatureUsageData
    {
        public int exploreUsage;
        public int craftUsage;
        public int orderUsage;
        public int achievementUsage;
    }
}