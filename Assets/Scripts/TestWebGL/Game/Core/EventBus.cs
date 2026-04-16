using System;
using System.Collections.Generic;

namespace SaveWorld.Game.Core
{
    /// <summary>
    /// 事件总线
    /// 全局唯一事件分发中心
    /// 所有跨层通信唯一通道
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

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
        /// 发布事件
        /// </summary>
        public void Publish<T>(T eventData) where T : GameEvent
        {
            Type eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    ((Action<T>)handler)(eventData);
                }
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : GameEvent
        {
            Type eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        /// <summary>
        /// 清空所有事件订阅
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
        }
    }
}