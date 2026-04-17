using System;
using System.Collections.Generic;

using SaveWorld.Game.Items;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.Order
{
    /// <summary>
    /// 订单管理器
    /// 管理订单生成、刷新、提交
    /// 设计规范第五节
    /// </summary>
    public class OrderManager
    {
        private static OrderManager _instance;
        public static OrderManager Instance => _instance ??= new OrderManager();

        private Random _random = new Random();
        private List<OrderData> _activeOrders = new List<OrderData>();

        // 配置常量
        private const int MAX_ACTIVE_ORDERS = 3;
        private const int ORDER_EXPIRE_HOURS = 6;
        private const int DAILY_REFRESH_LIMIT = 3;

        /// <summary>
        /// 订单提交成功事件
        /// </summary>
        public event Action<OrderData> OnOrderCompleted;

        /// <summary>
        /// 订单列表更新事件
        /// </summary>
        public event Action OnOrdersUpdated;

        private OrderManager()
        {
            // 监听事件总线
            GameLoop.Instance.EventBus.Listen<OrderSubmittedEvent>(OnOrderSubmitted);
        }

        /// <summary>
        /// 初始化订单系统
        /// </summary>
        public void Initialize()
        {
            if (_activeOrders.Count == 0)
            {
                GenerateNewOrders(MAX_ACTIVE_ORDERS);
            }

            CleanExpiredOrders();
            UnityEngine.Debug.Log($"[OrderManager] 初始化完成，当前有效订单: {_activeOrders.Count}");
        }

        /// <summary>
        /// 生成新订单
        /// </summary>
        public void GenerateNewOrders(int count)
        {
            int playerLevel = PlayerManager.Instance.GetLevel();

            for (int i = 0; i < count; i++)
            {
                if (_activeOrders.Count >= MAX_ACTIVE_ORDERS)
                    break;

                var order = GenerateSingleOrder(playerLevel);
                _activeOrders.Add(order);
            }

            OnOrdersUpdated?.Invoke();
        }

        /// <summary>
        /// 生成单个订单
        /// </summary>
        private OrderData GenerateSingleOrder(int playerLevel)
        {
            // 根据玩家等级确定可生成的物品等级
            int itemLevel = Mathf.Min((playerLevel / 10) + 1, 10);

            // 从对应等级物品池中随机选择
            ItemType[] availableItems = GetAvailableItemsForLevel(itemLevel);
            ItemType selectedItem = availableItems[_random.Next(availableItems.Length)];

            // 计算奖励
            int baseExp = 20 + playerLevel * 5;
            int baseGold = 10 + playerLevel * 3;

            return new OrderData
            {
                OrderId = Guid.NewGuid().GetHashCode(),
                RequireItem = selectedItem,
                RewardExp = baseExp,
                RewardGold = baseGold,
                CreateTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddHours(ORDER_EXPIRE_HOURS),
                IsCompleted = false,
                IsClaimed = false
            };
        }

        /// <summary>
        /// 获取指定等级可用物品
        /// </summary>
        private ItemType[] GetAvailableItemsForLevel(int level)
        {
            // TODO: 完整物品池配置
            return new[]
            {
                ItemType.Water_L1,
                ItemType.Food_L1,
                ItemType.Tool_L1,
                ItemType.Home_L1,
                ItemType.Medical_L1
            };
        }

        /// <summary>
        /// 尝试提交订单
        /// </summary>
        public bool TrySubmitOrder(int orderId)
        {
            var order = _activeOrders.Find(o => o.OrderId == orderId);

            if (order == null || order.IsCompleted || order.IsClaimed)
                return false;

            if (DateTime.Now > order.ExpireTime)
                return false;

            // 检查背包是否有需要的物品
            if (!Grid.GridManager.Instance.HasItem(order.RequireItem))
                return false;

            // 消耗物品
            Grid.GridManager.Instance.RemoveItem(order.RequireItem);

            // 标记完成
            order.IsCompleted = true;

            // 发布事件
            GameLoop.Instance.EventBus.Publish(new OrderSubmittedEvent(
                orderId,
                order.RewardGold,
                order.RewardExp
            ));

            OnOrderCompleted?.Invoke(order);
            OnOrdersUpdated?.Invoke();

            UnityEngine.Debug.Log($"[OrderManager] 订单 {orderId} 提交成功，获得 {order.RewardExp} 经验, {order.RewardGold} 金币");

            return true;
        }

        /// <summary>
        /// 领取订单奖励
        /// </summary>
        public bool ClaimOrderReward(int orderId)
        {
            var order = _activeOrders.Find(o => o.OrderId == orderId);

            if (order == null || !order.IsCompleted || order.IsClaimed)
                return false;

            order.IsClaimed = true;

            // 发放奖励
            GameLoop.Instance.EventBus.Publish(new ExperienceGainedEvent(order.RewardExp, "订单奖励"));

            // 移除已完成订单
            _activeOrders.Remove(order);

            // 生成新订单补充
            GenerateNewOrders(1);

            OnOrdersUpdated?.Invoke();

            return true;
        }

        /// <summary>
        /// 刷新订单（消耗钻石/广告）
        /// </summary>
        public bool RefreshOrder(int orderId)
        {
            var order = _activeOrders.Find(o => o.OrderId == orderId);

            if (order == null || order.IsCompleted)
                return false;

            _activeOrders.Remove(order);
            GenerateNewOrders(1);

            return true;
        }

        /// <summary>
        /// 清理过期订单
        /// </summary>
        public void CleanExpiredOrders()
        {
            int removed = _activeOrders.RemoveAll(o =>
                DateTime.Now > o.ExpireTime && !o.IsCompleted);

            if (removed > 0)
            {
                GenerateNewOrders(removed);
                UnityEngine.Debug.Log($"[OrderManager] 清理了 {removed} 个过期订单");
            }
        }

        /// <summary>
        /// 获取当前有效订单列表
        /// </summary>
        public List<OrderData> GetActiveOrders()
        {
            CleanExpiredOrders();
            return new List<OrderData>(_activeOrders);
        }

        /// <summary>
        /// 检查今日剩余刷新次数
        /// </summary>
        public int GetRemainingDailyRefreshes()
        {
            // TODO: 从玩家数据读取
            return DAILY_REFRESH_LIMIT;
        }

        private void OnOrderSubmitted(OrderSubmittedEvent e)
        {
            // 事件处理
        }
    }
}
