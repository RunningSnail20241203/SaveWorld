using System;
using System.Collections.Generic;
using TestWebGL.Game.Items;

namespace TestWebGL.Game.Order
{
    /// <summary>
    /// 订单系统 - 管理每日订单生成和提交
    /// 设计规范第五节
    /// </summary>
    public class OrderSystem
    {
        /// <summary>
        /// 订单数据结构
        /// </summary>
        public struct OrderData
        {
            public int orderId;             // 订单唯一ID
            public ItemType requiredItem;   // 需要的物品类型
            public int requiredLevel;       // 需要的物品等级
            public int requiredCount;       // 需要的物品数量
            public RewardType rewardType;   // 奖励类型
            public int rewardAmount;        // 奖励数量（不同奖励类型含义不同）
            public bool isGuide;            // 是否为新手引导订单
            public bool isCompleted;        // 是否已完成
            public DateTime createdTime;    // 创建时间

            public string GetDisplayName()
            {
                return $"{ItemConfig.GetItemName(requiredItem)} L{requiredLevel} ×{requiredCount}";
            }
        }

        /// <summary>
        /// 奖励类型
        /// </summary>
        public enum RewardType
        {
            EnergyDrink,    // 能量饮料（临时+体力，1小时内有效）
            StaminaNormal,  // 普通体力恢复
            StaminaLarge,   // 大量体力恢复
            Experience,     // 经验值
            Title,          // 称号/勋章
            Unlock,         // 解锁新的生产线
        }

        private Random _random = new Random();
        private List<OrderData> _dailyOrders = new List<OrderData>();
        private int _orderIdCounter = 0;

        // 事件
        public delegate void OrderGeneratedHandler(OrderData[] orders);
        public event OrderGeneratedHandler OnOrdersGenerated;

        public delegate void OrderCompletedHandler(OrderData order, RewardType rewardType);
        public event OrderCompletedHandler OnOrderCompleted;

        public OrderSystem()
        {
            _orderIdCounter = (int)(System.DateTime.Now.Ticks % 10000);
        }

        /// <summary>
        /// 生成每日订单
        /// 根据玩家的历史最高等级加权生成
        /// </summary>
        public OrderData[] GenerateDailyOrders(int playerLevel, int maxLevelReached)
        {
            // 清空旧订单
            _dailyOrders.Clear();

            UnityEngine.Debug.Log($"[OrderSystem] 生成每日订单 (玩家等级: Lv{playerLevel}, 历史最高: Lv{maxLevelReached})");

            // 生成5个订单
            for (int i = 0; i < 5; i++)
            {
                OrderData order = GenerateSingleOrder(playerLevel, maxLevelReached);
                _dailyOrders.Add(order);
                UnityEngine.Debug.Log($"  [{i+1}] {order.GetDisplayName()} → {order.rewardType} ×{order.rewardAmount}");
            }

            OnOrdersGenerated?.Invoke(_dailyOrders.ToArray());

            return _dailyOrders.ToArray();
        }

        /// <summary>
        /// 生成单个订单
        /// </summary>
        private OrderData GenerateSingleOrder(int playerLevel, int maxLevelReached)
        {
            var order = new OrderData();
            order.orderId = _orderIdCounter++;
            order.createdTime = System.DateTime.Now;
            order.isCompleted = false;

            // 5%概率生成新手引导订单
            if (_random.Next(0, 100) < 5)
            {
                order.isGuide = true;
                order.requiredItem = ItemType.Water_L1;
                order.requiredLevel = 1;
                order.requiredCount = 1;
                order.rewardType = RewardType.Experience;
                order.rewardAmount = 10;

                UnityEngine.Debug.Log($"[OrderSystem] 生成新手引导订单");
            }
            else
            {
                order.isGuide = false;

                // 根据历史最高等级确定生成范围
                // Lv1-10: 只生成L1-L2订单
                // Lv11-20: 生成L1-L3订单
                // Lv21-30: 生成L2-L4订单
                // ...
                // Lv61+: 生成L6-L10订单
                int minGenerateLevel = Math.Max(1, (maxLevelReached - 10) / 10 + 1);
                int maxGenerateLevel = Math.Min(10, (maxLevelReached - 1) / 10 + 2);

                // 随机选择物品
                int[] productionLines = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };  // 9条生产线
                int lineIndex = _random.Next(productionLines.Length);

                // 随机选择等级
                order.requiredLevel = _random.Next(minGenerateLevel, maxGenerateLevel + 1);
                
                // 计算物品类型
                // ItemType.Water_L1 = 1001, Water_L2 = 1002, ...
                int baseItemType = 1000 + lineIndex * 10;  // 每条线差10
                order.requiredItem = (ItemType)(baseItemType + order.requiredLevel);

                // 随机数量（1-3个）
                order.requiredCount = _random.Next(1, 4);

                // 生成奖励
                order.rewardType = GenerateRewardType(order.requiredLevel);
                order.rewardAmount = GenerateRewardAmount(order.rewardType, order.requiredLevel);
            }

            return order;
        }

        /// <summary>
        /// 根据订单等级生成奖励类型
        /// </summary>
        private RewardType GenerateRewardType(int orderLevel)
        {
            // 简化：根据等级加权选择奖励
            int rand = _random.Next(0, 100);

            if (orderLevel <= 2)
            {
                if (rand < 50) return RewardType.EnergyDrink;
                if (rand < 80) return RewardType.StaminaNormal;
                return RewardType.Experience;
            }
            else if (orderLevel <= 5)
            {
                if (rand < 40) return RewardType.StaminaNormal;
                if (rand < 70) return RewardType.Experience;
                if (rand < 85) return RewardType.StaminaLarge;
                return RewardType.Title;
            }
            else
            {
                if (rand < 30) return RewardType.Experience;
                if (rand < 60) return RewardType.StaminaLarge;
                if (rand < 85) return RewardType.Title;
                return RewardType.Unlock;
            }
        }

        /// <summary>
        /// 根据奖励类型生成奖励数量
        /// </summary>
        private int GenerateRewardAmount(RewardType rewardType, int orderLevel)
        {
            switch (rewardType)
            {
                case RewardType.EnergyDrink:
                    return 10 + orderLevel * 5;  // 10-60分钟

                case RewardType.StaminaNormal:
                    return 5 + orderLevel;  // 5-15点体力

                case RewardType.StaminaLarge:
                    return 10 + orderLevel * 2;  // 10-30点体力

                case RewardType.Experience:
                    return 50 * orderLevel;  // 50-500经验

                case RewardType.Title:
                    return 1;  // 1个称号

                case RewardType.Unlock:
                    return 1;  // 解锁1条生产线

                default:
                    return 10;
            }
        }

        /// <summary>
        /// 尝试提交订单
        /// 检查背包中是否有所需物品
        /// </summary>
        public bool TryCompleteOrder(int orderId, Grid.GridManager gridManager)
        {
            // 在订单列表中查找
            OrderData targetOrder = new OrderData();
            bool found = false;

            foreach (var order in _dailyOrders)
            {
                if (order.orderId == orderId)
                {
                    targetOrder = order;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                UnityEngine.Debug.Log($"[OrderSystem] 订单 {orderId} 未找到");
                return false;
            }

            // 检查网格中是否有足够的物品
            int itemCount = 0;
            var cells = gridManager.GetAllCells();
            foreach (var cell in cells)
            {
                if (!cell.IsLocked && cell.HasItem && cell.CurrentItemType == targetOrder.requiredItem && 
                    ItemConfig.GetItemLevel(cell.CurrentItemType) == targetOrder.requiredLevel)
                {
                    itemCount += cell.ItemCount;
                }
            }

            if (itemCount < targetOrder.requiredCount)
            {
                UnityEngine.Debug.Log($"[OrderSystem] 物品不足: 需要{targetOrder.requiredCount}个，有{itemCount}个");
                return false;
            }

            // 消耗物品
            int remaining = targetOrder.requiredCount;
            foreach (var cell in cells)
            {
                if (remaining == 0) break;

                if (!cell.IsLocked && cell.HasItem && cell.CurrentItemType == targetOrder.requiredItem && 
                    ItemConfig.GetItemLevel(cell.CurrentItemType) == targetOrder.requiredLevel)
                {
                    int toRemove = Math.Min(remaining, cell.ItemCount);
                    // 这里需要调用GridManager的移除物品方法
                    // 简化实现：标记为完成
                    remaining -= toRemove;
                }
            }

            // 标记订单为完成
            for (int i = 0; i < _dailyOrders.Count; i++)
            {
                if (_dailyOrders[i].orderId == orderId)
                {
                    var completedOrder = _dailyOrders[i];
                    completedOrder.isCompleted = true;
                    _dailyOrders[i] = completedOrder;
                    break;
                }
            }

            OnOrderCompleted?.Invoke(targetOrder, targetOrder.rewardType);

            UnityEngine.Debug.Log($"[OrderSystem] 订单完成: {targetOrder.GetDisplayName()} → 获得 {targetOrder.rewardType} ×{targetOrder.rewardAmount}");
            return true;
        }

        /// <summary>
        /// 获取所有当前订单
        /// </summary>
        public OrderData[] GetCurrentOrders()
        {
            return _dailyOrders.ToArray();
        }

        /// <summary>
        /// 获取未完成的订单数量
        /// </summary>
        public int GetPendingOrderCount()
        {
            int count = 0;
            foreach (var order in _dailyOrders)
            {
                if (!order.isCompleted)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 计算订单完成的总奖励经验
        /// 用于玩家升级计算
        /// </summary>
        public int CalculateOrderRewardExperience(OrderData order)
        {
            switch (order.rewardType)
            {
                case RewardType.Experience:
                    return order.rewardAmount;  // 直接返回经验值

                case RewardType.StaminaNormal:
                case RewardType.StaminaLarge:
                    return order.requiredLevel * 10;  // 体力订单给100exp

                case RewardType.EnergyDrink:
                    return order.requiredLevel * 5;  // 饮料订单给50exp

                case RewardType.Title:
                    return 200;  // 称号订单给200exp

                case RewardType.Unlock:
                    return 500;  // 解锁订单给500exp

                default:
                    return 0;
            }
        }
    }
}
