using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestWebGL.Game.Operations
{
    /// <summary>
    /// 活动系统
    /// 负责游戏内活动的管理、奖励分发等功能
    /// </summary>
    public class EventSystem : MonoBehaviour
    {
        private static EventSystem s_instance;
        public static EventSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("EventSystem");
                    s_instance = go.AddComponent<EventSystem>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 活动配置
        private const string EVENT_KEY = "event_data";
        private const string EVENT_PROGRESS_KEY = "event_progress";
        private const float EVENT_CHECK_INTERVAL = 60f; // 1分钟检查一次

        // 活动数据
        private List<EventData> _activeEvents = new List<EventData>();
        private Dictionary<string, EventProgress> _eventProgress = new Dictionary<string, EventProgress>();
        
        // 检查计时器
        private float _eventCheckTimer = 0f;
        
        // 初始化状态
        private bool _isInitialized = false;

        // 事件
        public event Action<EventData> OnEventStarted;
        public event Action<EventData> OnEventEnded;
        public event Action<EventData, EventReward> OnEventRewardClaimed;
        public event Action<string, int> OnEventProgressUpdated;

        /// <summary>
        /// 初始化活动系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[EventSystem] 初始化活动系统...");

            // 加载活动数据
            LoadEventData();
            LoadEventProgress();

            // 检查活动状态
            CheckEventStatus();

            _isInitialized = true;
            Debug.Log("[EventSystem] 活动系统初始化完成");
        }

        /// <summary>
        /// 加载活动数据
        /// </summary>
        private void LoadEventData()
        {
            try
            {
                if (PlayerPrefs.HasKey(EVENT_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(EVENT_KEY);
                    var wrapper = JsonUtility.FromJson<EventDataWrapper>(jsonData);
                    if (wrapper != null && wrapper.events != null)
                    {
                        _activeEvents = wrapper.events;
                    }
                }

                // 如果没有活动数据，创建默认活动
                if (_activeEvents.Count == 0)
                {
                    CreateDefaultEvents();
                }

                Debug.Log($"[EventSystem] 活动数据加载成功: {_activeEvents.Count}个活动");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventSystem] 加载活动数据失败: {ex.Message}");
                CreateDefaultEvents();
            }
        }

        /// <summary>
        /// 创建默认活动
        /// </summary>
        private void CreateDefaultEvents()
        {
            _activeEvents = new List<EventData>
            {
                new EventData
                {
                    eventId = "daily_login",
                    eventName = "每日登录",
                    description = "每天登录游戏即可获得奖励",
                    eventType = EventType.Daily,
                    startTime = DateTime.Now.Date,
                    endTime = DateTime.Now.Date.AddDays(1).AddSeconds(-1),
                    isActive = true,
                    rewards = new List<EventReward>
                    {
                        new EventReward { rewardType = "stamina", amount = 5, description = "体力×5" }
                    },
                    requirements = new List<EventRequirement>
                    {
                        new EventRequirement { requirementType = "login", targetValue = 1 }
                    }
                },
                new EventData
                {
                    eventId = "weekly_explore",
                    eventName = "每周探索",
                    description = "本周内完成10次探索",
                    eventType = EventType.Weekly,
                    startTime = GetWeekStart(),
                    endTime = GetWeekStart().AddDays(7).AddSeconds(-1),
                    isActive = true,
                    rewards = new List<EventReward>
                    {
                        new EventReward { rewardType = "item", amount = 3, description = "探索加速券×3" }
                    },
                    requirements = new List<EventRequirement>
                    {
                        new EventRequirement { requirementType = "explore", targetValue = 10 }
                    }
                },
                new EventData
                {
                    eventId = "craft_master",
                    eventName = "合成大师",
                    description = "累计合成50次物品",
                    eventType = EventType.Permanent,
                    startTime = DateTime.Now.AddYears(-1),
                    endTime = DateTime.Now.AddYears(1),
                    isActive = true,
                    rewards = new List<EventReward>
                    {
                        new EventReward { rewardType = "achievement", amount = 1, description = "合成大师成就" }
                    },
                    requirements = new List<EventRequirement>
                    {
                        new EventRequirement { requirementType = "craft", targetValue = 50 }
                    }
                }
            };

            SaveEventData();
        }

        /// <summary>
        /// 获取周开始时间
        /// </summary>
        private DateTime GetWeekStart()
        {
            DateTime today = DateTime.Now.Date;
            int daysToSubtract = (int)today.DayOfWeek;
            if (daysToSubtract == 0) daysToSubtract = 7; // 周日
            return today.AddDays(-daysToSubtract + 1); // 周一
        }

        /// <summary>
        /// 加载活动进度
        /// </summary>
        private void LoadEventProgress()
        {
            try
            {
                if (PlayerPrefs.HasKey(EVENT_PROGRESS_KEY))
                {
                    string jsonData = PlayerPrefs.GetString(EVENT_PROGRESS_KEY);
                    var wrapper = JsonUtility.FromJson<EventProgressWrapper>(jsonData);
                    if (wrapper != null && wrapper.progress != null)
                    {
                        _eventProgress = new Dictionary<string, EventProgress>();
                        foreach (var progress in wrapper.progress)
                        {
                            _eventProgress[progress.eventId] = progress;
                        }
                    }
                }

                Debug.Log($"[EventSystem] 活动进度加载成功: {_eventProgress.Count}个进度");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventSystem] 加载活动进度失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存活动数据
        /// </summary>
        private void SaveEventData()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(new EventDataWrapper { events = _activeEvents });
                PlayerPrefs.SetString(EVENT_KEY, jsonData);
                PlayerPrefs.Save();
                Debug.Log("[EventSystem] 活动数据保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventSystem] 保存活动数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存活动进度
        /// </summary>
        private void SaveEventProgress()
        {
            try
            {
                var progressList = new List<EventProgress>(_eventProgress.Values);
                string jsonData = JsonUtility.ToJson(new EventProgressWrapper { progress = progressList });
                PlayerPrefs.SetString(EVENT_PROGRESS_KEY, jsonData);
                PlayerPrefs.Save();
                Debug.Log("[EventSystem] 活动进度保存成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventSystem] 保存活动进度失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查活动状态
        /// </summary>
        private void CheckEventStatus()
        {
            DateTime now = DateTime.Now;

            foreach (var eventData in _activeEvents)
            {
                bool shouldBeActive = now >= eventData.startTime && now <= eventData.endTime;

                if (shouldBeActive && !eventData.isActive)
                {
                    // 活动开始
                    eventData.isActive = true;
                    OnEventStarted?.Invoke(eventData);
                    Debug.Log($"[EventSystem] 活动开始: {eventData.eventName}");
                }
                else if (!shouldBeActive && eventData.isActive)
                {
                    // 活动结束
                    eventData.isActive = false;
                    OnEventEnded?.Invoke(eventData);
                    Debug.Log($"[EventSystem] 活动结束: {eventData.eventName}");
                }
            }

            SaveEventData();
        }

        /// <summary>
        /// 更新活动进度
        /// </summary>
        public void UpdateEventProgress(string requirementType, int amount = 1)
        {
            if (!_isInitialized) return;

            foreach (var eventData in _activeEvents)
            {
                if (!eventData.isActive) continue;

                foreach (var requirement in eventData.requirements)
                {
                    if (requirement.requirementType == requirementType)
                    {
                        // 获取或创建进度
                        if (!_eventProgress.TryGetValue(eventData.eventId, out var progress))
                        {
                            progress = new EventProgress
                            {
                                eventId = eventData.eventId,
                                currentValues = new Dictionary<string, int>()
                            };
                            _eventProgress[eventData.eventId] = progress;
                        }

                        // 更新进度
                        if (!progress.currentValues.ContainsKey(requirementType))
                        {
                            progress.currentValues[requirementType] = 0;
                        }

                        progress.currentValues[requirementType] += amount;

                        // 检查是否完成
                        bool isCompleted = true;
                        foreach (var req in eventData.requirements)
                        {
                            int currentValue = progress.currentValues.ContainsKey(req.requirementType) 
                                ? progress.currentValues[req.requirementType] : 0;
                            if (currentValue < req.targetValue)
                            {
                                isCompleted = false;
                                break;
                            }
                        }

                        progress.isCompleted = isCompleted;

                        OnEventProgressUpdated?.Invoke(eventData.eventId, progress.currentValues[requirementType]);

                        Debug.Log($"[EventSystem] 活动进度更新: {eventData.eventName} - {requirementType}: {progress.currentValues[requirementType]}/{requirement.targetValue}");
                    }
                }
            }

            SaveEventProgress();
        }

        /// <summary>
        /// 领取活动奖励
        /// </summary>
        public void ClaimEventReward(string eventId)
        {
            if (!_isInitialized) return;

            var eventData = _activeEvents.Find(e => e.eventId == eventId);
            if (eventData == null)
            {
                Debug.LogWarning($"[EventSystem] 活动不存在: {eventId}");
                return;
            }

            if (!_eventProgress.TryGetValue(eventId, out var progress) || !progress.isCompleted)
            {
                Debug.LogWarning($"[EventSystem] 活动未完成: {eventId}");
                return;
            }

            if (progress.isRewardClaimed)
            {
                Debug.LogWarning($"[EventSystem] 奖励已领取: {eventId}");
                return;
            }

            // 发放奖励
            foreach (var reward in eventData.rewards)
            {
                GrantReward(reward);
                OnEventRewardClaimed?.Invoke(eventData, reward);
                Debug.Log($"[EventSystem] 发放奖励: {reward.description}");
            }

            // 标记奖励已领取
            progress.isRewardClaimed = true;
            SaveEventProgress();
        }

        /// <summary>
        /// 发放奖励
        /// </summary>
        private void GrantReward(EventReward reward)
        {
            // 这里实现实际的奖励发放逻辑
            // 可以调用GameManager的相关方法
            Debug.Log($"[EventSystem] 发放奖励: {reward.rewardType} ×{reward.amount}");
        }

        /// <summary>
        /// 获取活跃活动列表
        /// </summary>
        public List<EventData> GetActiveEvents()
        {
            return _activeEvents.FindAll(e => e.isActive);
        }

        /// <summary>
        /// 获取所有活动
        /// </summary>
        public List<EventData> GetAllEvents()
        {
            return new List<EventData>(_activeEvents);
        }

        /// <summary>
        /// 获取活动进度
        /// </summary>
        public EventProgress GetEventProgress(string eventId)
        {
            return _eventProgress.TryGetValue(eventId, out var progress) ? progress : null;
        }

        /// <summary>
        /// 获取可领取的奖励
        /// </summary>
        public List<EventData> GetClaimableEvents()
        {
            var claimable = new List<EventData>();
            foreach (var eventData in _activeEvents)
            {
                if (eventData.isActive && _eventProgress.TryGetValue(eventData.eventId, out var progress))
                {
                    if (progress.isCompleted && !progress.isRewardClaimed)
                    {
                        claimable.Add(eventData);
                    }
                }
            }
            return claimable;
        }

        /// <summary>
        /// 更新方法
        /// </summary>
        private void Update()
        {
            if (!_isInitialized) return;

            // 定时检查活动状态
            _eventCheckTimer += Time.deltaTime;
            if (_eventCheckTimer >= EVENT_CHECK_INTERVAL)
            {
                _eventCheckTimer = 0f;
                CheckEventStatus();
            }
        }

        /// <summary>
        /// 获取活动统计信息
        /// </summary>
        public string GetEventInfo()
        {
            var activeEvents = GetActiveEvents();
            var claimableEvents = GetClaimableEvents();
            return $"活跃活动: {activeEvents.Count}个, 可领取: {claimableEvents.Count}个";
        }

        /// <summary>
        /// 清除所有活动数据
        /// </summary>
        public void ClearAllData()
        {
            _activeEvents.Clear();
            _eventProgress.Clear();
            PlayerPrefs.DeleteKey(EVENT_KEY);
            PlayerPrefs.DeleteKey(EVENT_PROGRESS_KEY);
            PlayerPrefs.Save();
            Debug.Log("[EventSystem] 所有活动数据已清除");
        }
    }

    /// <summary>
    /// 活动类型
    /// </summary>
    public enum EventType
    {
        Daily,      // 每日活动
        Weekly,     // 每周活动
        Monthly,    // 每月活动
        Permanent,  // 永久活动
        Special     // 特殊活动
    }

    /// <summary>
    /// 活动数据
    /// </summary>
    [System.Serializable]
    public class EventData
    {
        public string eventId;
        public string eventName;
        public string description;
        public EventType eventType;
        public DateTime startTime;
        public DateTime endTime;
        public bool isActive;
        public List<EventReward> rewards;
        public List<EventRequirement> requirements;
    }

    /// <summary>
    /// 活动奖励
    /// </summary>
    [System.Serializable]
    public class EventReward
    {
        public string rewardType;
        public int amount;
        public string description;
    }

    /// <summary>
    /// 活动要求
    /// </summary>
    [System.Serializable]
    public class EventRequirement
    {
        public string requirementType;
        public int targetValue;
    }

    /// <summary>
    /// 活动进度
    /// </summary>
    [System.Serializable]
    public class EventProgress
    {
        public string eventId;
        public Dictionary<string, int> currentValues;
        public bool isCompleted;
        public bool isRewardClaimed;
    }

    /// <summary>
    /// 活动数据包装类
    /// </summary>
    [System.Serializable]
    public class EventDataWrapper
    {
        public List<EventData> events;
    }

    /// <summary>
    /// 活动进度包装类
    /// </summary>
    [System.Serializable]
    public class EventProgressWrapper
    {
        public List<EventProgress> progress;
    }
}