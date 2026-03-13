using System.Collections.Generic;
using UnityEngine;
using TestWebGL.Game.Core;

namespace TestWebGL.Game.Orders
{
    /// <summary>
    /// 订单管理器 - 管理游戏中的所有订单
    /// </summary>
    public class OrderManager
    {
        private List<OrderData> _activeOrders = new List<OrderData>();
        private List<OrderData> _completedOrders = new List<OrderData>();
        private int _nextOrderId = 1;

        /// <summary>
        /// 初始化订单管理器
        /// </summary>
        public void Initialize()
        {
            // 创建一些示例订单
            CreateSampleOrders();
            Debug.Log("[OrderManager] 订单管理器初始化完成");
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        public void Update(float deltaTime)
        {
            // 更新活跃订单
            for (int i = _activeOrders.Count - 1; i >= 0; i--)
            {
                _activeOrders[i].Update(deltaTime);
                if (_activeOrders[i].IsCompleted)
                {
                    _completedOrders.Add(_activeOrders[i]);
                    _activeOrders.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 获取活跃订单列表
        /// </summary>
        public List<OrderData> GetActiveOrders()
        {
            return new List<OrderData>(_activeOrders);
        }

        /// <summary>
        /// 获取已完成订单列表
        /// </summary>
        public List<OrderData> GetCompletedOrders()
        {
            return new List<OrderData>(_completedOrders);
        }

        /// <summary>
        /// 领取订单奖励
        /// </summary>
        public bool ClaimOrderReward(string orderId)
        {
            OrderData order = _completedOrders.Find(o => o.Id == orderId);
            if (order != null && order.ClaimReward())
            {
                // 给予玩家奖励
                var playerManager = GameManager.Instance.GetPlayerManager();
                if (playerManager != null)
                {
                    playerManager.GainExperience(order.Reward, "Order Reward");
                    Debug.Log($"[OrderManager] 领取订单奖励: {order.Title}, 获得经验: {order.Reward}");
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 添加新订单
        /// </summary>
        public void AddOrder(OrderData order)
        {
            _activeOrders.Add(order);
            Debug.Log($"[OrderManager] 添加新订单: {order.Title}");
        }

        /// <summary>
        /// 创建示例订单
        /// </summary>
        private void CreateSampleOrders()
        {
            // 创建一些示例订单
            AddOrder(new OrderData(
                GenerateOrderId(),
                "收集木材",
                "收集10个木材用于建造",
                50,
                300f, // 5分钟
                OrderType.Normal
            ));

            AddOrder(new OrderData(
                GenerateOrderId(),
                "合成铁锭",
                "将铁矿石合成铁锭",
                75,
                600f, // 10分钟
                OrderType.Normal
            ));

            AddOrder(new OrderData(
                GenerateOrderId(),
                "紧急任务：收集水晶",
                "立即收集稀有水晶",
                200,
                180f, // 3分钟
                OrderType.Urgent
            ));

            // 添加一些已完成的订单
            OrderData completedOrder1 = new OrderData(
                GenerateOrderId(),
                "完成的基础任务",
                "这是一个已完成的基础任务",
                25,
                0f,
                OrderType.Normal
            );
            completedOrder1.IsCompleted = true;
            _completedOrders.Add(completedOrder1);

            OrderData completedOrder2 = new OrderData(
                GenerateOrderId(),
                "高级合成任务",
                "合成高级物品的任务",
                150,
                0f,
                OrderType.Special
            );
            completedOrder2.IsCompleted = true;
            _completedOrders.Add(completedOrder2);
        }

        /// <summary>
        /// 生成订单ID
        /// </summary>
        private string GenerateOrderId()
        {
            return $"order_{_nextOrderId++}";
        }

        /// <summary>
        /// 获取活跃订单数量
        /// </summary>
        public int GetActiveOrderCount()
        {
            return _activeOrders.Count;
        }

        /// <summary>
        /// 获取已完成但未领取的订单数量
        /// </summary>
        public int GetUnclaimedOrderCount()
        {
            return _completedOrders.FindAll(o => !o.IsClaimed).Count;
        }
    }
}