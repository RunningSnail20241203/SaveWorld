using System;
using System.Collections.Generic;

namespace SaveWorld.Game.Core
{
    /// <summary>
    /// 事件基类
    /// 所有游戏事件都需要继承此类
    /// </summary>
    public abstract class GameEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 事件总线
    /// 全局唯一事件分发中心
    /// 所有跨层通信唯一通道
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly Queue<GameEvent> _eventQueue = new Queue<GameEvent>();

        /// <summary>
        /// 订阅事件
        /// </summary>
        public void Listen<T>(Action<T> handler) where T : GameEvent
        {
            Type eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }
            _handlers[eventType].Add(handler);
        }

        /// <summary>
        /// 发布事件 (加入队列)
        /// </summary>
        public void Publish<T>(T eventData) where T : GameEvent
        {
            _eventQueue.Enqueue(eventData);
        }

        /// <summary>
        /// 发布事件 (别名，兼容旧代码)
        /// </summary>
        public void Dispatch<T>(T eventData) where T : GameEvent
        {
            Publish(eventData);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : GameEvent
        {
            Type eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out List<Delegate> handlers))
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// 处理所有待处理事件
        /// 在主线程每帧调用
        /// </summary>
        public void ProcessEvents()
        {
            while (_eventQueue.Count > 0)
            {
                GameEvent gameEvent = _eventQueue.Dequeue();
                Type eventType = gameEvent.GetType();
                
                if (_handlers.TryGetValue(eventType, out List<Delegate> handlers))
                {
                    foreach (var handler in handlers)
                    {
                        handler.DynamicInvoke(gameEvent);
                    }
                }
            }
        }

        /// <summary>
        /// 清空所有事件订阅
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
            _eventQueue.Clear();
        }
    }

    /// <summary>
    /// 体力领取事件
    /// </summary>
    public sealed class StaminaClaimedEvent : GameEvent
    {
        public int Amount { get; set; }
        public int NewStamina { get; set; }
    }

    /// <summary>
    /// 云同步开始事件
    /// </summary>
    public sealed class CloudSyncStartedEvent : GameEvent
    {
        public string SyncId { get; set; }
    }

    /// <summary>
    /// 云同步完成事件
    /// </summary>
    public sealed class CloudSyncCompletedEvent : GameEvent
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 音效播放事件
    /// </summary>
    public sealed class SFXPlayedEvent : GameEvent
    {
        public int SoundId { get; set; }
        public float Volume { get; set; }
    }

}