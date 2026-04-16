using System;
using System.Collections.Generic;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.Analytics
{
    /// <summary>
    /// 数据分析系统
    /// 自动收集玩家行为数据 用于运营分析
    /// </summary>
    public class AnalyticsSystem
    {
        private readonly EventBus _eventBus;
        private readonly Queue<AnalyticsEvent> _eventQueue;
        private const int MAX_QUEUE_SIZE = 100;
        private bool _initialized = false;
        private string _sessionId;

        public AnalyticsSystem(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventQueue = new Queue<AnalyticsEvent>();
            _sessionId = Guid.NewGuid().ToString();
            
            RegisterEventHandlers();
            TrackSessionStart();
        }

        private void RegisterEventHandlers()
        {
            // 自动监听所有游戏事件
            _eventBus.Listen<MergeCompleteEvent>(e => LogEvent("merge_complete", 
                new Dictionary<string, object> { {"item_id", e.NewItemId} }));

            _eventBus.Listen<ExplorationCompleteEvent>(e => LogEvent("exploration_complete",
                new Dictionary<string, object> { {"items_generated", e.GeneratedCellIds.Length} }));

            _eventBus.Listen<OrderSubmittedEvent>(e => LogEvent("order_submitted",
                new Dictionary<string, object> 
                { 
                    {"order_id", e.OrderId},
                    {"exp_reward", e.ExpReward},
                    {"coin_reward", e.CoinReward}
                }));

            _eventBus.Listen<AchievementUnlockedEvent>(e => LogEvent("achievement_unlocked",
                new Dictionary<string, object> { {"achievement_id", e.AchievementId} }));

            _eventBus.Listen<LevelUpEvent>(e => LogEvent("level_up",
                new Dictionary<string, object> { {"new_level", e.NewLevel} }));

            _eventBus.Listen<ItemMovedEvent>(e => LogEvent("item_moved"));
        }

        /// <summary>
        /// 记录自定义事件
        /// </summary>
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            var analyticsEvent = new AnalyticsEvent
            {
                EventName = eventName,
                Timestamp = DateTime.UtcNow,
                SessionId = _sessionId,
                Parameters = parameters ?? new Dictionary<string, object>()
            };

            lock (_eventQueue)
            {
                if (_eventQueue.Count >= MAX_QUEUE_SIZE)
                {
                    _eventQueue.Dequeue();
                }
                _eventQueue.Enqueue(analyticsEvent);
            }

            _eventBus.Publish(new AnalyticsEventLoggedEvent
            {
                EventName = eventName,
                EventData = analyticsEvent
            });
        }

        private void TrackSessionStart()
        {
            LogEvent("session_start", new Dictionary<string, object>
            {
                {"session_id", _sessionId},
                {"timestamp", DateTime.UtcNow.ToString("o")}
            });
        }

        public void TrackSessionEnd()
        {
            LogEvent("session_end");
            FlushEvents();
        }

        /// <summary>
        /// 上报所有队列中的事件
        /// </summary>
        public void FlushEvents()
        {
            // TODO: 实际项目中上传到服务器
            _eventQueue.Clear();
        }

        /// <summary>
        /// 获取分析统计信息
        /// </summary>
        public string GetAnalyticsInfo()
        {
            return $"分析系统: 会话ID={_sessionId}, 事件队列={_eventQueue.Count}/{MAX_QUEUE_SIZE}";
        }
    }

    #region 数据结构

    [Serializable]
    public struct AnalyticsEvent
    {
        public string EventName;
        public DateTime Timestamp;
        public string SessionId;
        public Dictionary<string, object> Parameters;
    }

    #endregion

    #region 事件定义

    public class AnalyticsEventLoggedEvent : GameEvent
    {
        public string EventName;
        public AnalyticsEvent EventData;
    }

    #endregion
}