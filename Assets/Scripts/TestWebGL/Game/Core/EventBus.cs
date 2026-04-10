using System;
using System.Collections.Generic;

namespace SaveWorld.Game.Core
{
    /// <summary>
    /// 全局事件总线 - 简化版
    /// 仅保留最核心功能，无优先级、无可取消、无复杂防护
    /// 足够支撑99%的游戏需求
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscriptions = new Dictionary<Type, List<Delegate>>();
        private readonly Queue<Action> _eventQueue = new Queue<Action>();

        /// <summary>
        /// 监听指定类型的事件
        /// </summary>
        public void Listen<T>(Action<T> handler) where T : IGameEvent
        {
            var eventType = typeof(T);
            if (!_subscriptions.ContainsKey(eventType))
            {
                _subscriptions[eventType] = new List<Delegate>();
            }

            _subscriptions[eventType].Add(handler);
        }

        /// <summary>
        /// 发出事件（下一帧执行）
        /// </summary>
        public void Dispatch<T>(T @event) where T : IGameEvent
        {
            _eventQueue.Enqueue(() =>
            {
                var eventType = typeof(T);
                if (_subscriptions.TryGetValue(eventType, out var handlers))
                {
                    foreach (Action<T> handler in handlers)
                    {
                        try
                        {
                            handler(@event);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogError($"事件处理异常: {e}");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 执行所有排队的事件
        /// 每帧调用一次
        /// </summary>
        public void ProcessEvents()
        {
            while (_eventQueue.Count > 0)
            {
                _eventQueue.Dequeue().Invoke();
            }
        }
    }

    /// <summary>
    /// 所有游戏事件的基础接口
    /// </summary>
    public interface IGameEvent
    {
    }
}