using System;
using SaveWorld.Game.Core;
using SaveWorld.Game.Items;

namespace SaveWorld.Game.Order
{
    /// <summary>
    /// 订单引擎 V2
    /// 纯函数 无状态 无副作用 零依赖
    /// 输入: GameState 快照
    /// 输出: 订单操作结果
    /// 所有逻辑可预测 可单独测试
    /// </summary>
    public static class OrderEngine
    {
        private const int MAX_ACTIVE_ORDERS = 3;
        private const int ORDER_EXPIRE_HOURS = 6;

        /// <summary>
        /// 尝试提交订单
        /// 纯函数 不会修改任何状态 只会返回结果
        /// </summary>
        public static OrderResult TrySubmitOrder(GameState state, int orderId)
        {
            var order = FindOrder(state, orderId);
            
            if (order == null)
            {
                return OrderResult.Fail("订单不存在");
            }

            if (order.IsCompleted)
            {
                return OrderResult.Fail("订单已完成");
            }

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > order.ExpireTime)
            {
                return OrderResult.Fail("订单已过期");
            }

            // 检查背包是否有需要的物品
            int itemCellId = FindItemInBackpack(state, order.RequireItemId);
            
            if (itemCellId == -1)
            {
                return OrderResult.Fail("背包中没有需要的物品");
            }

            // 返回成功结果
            return OrderResult.Success(
                orderId,
                itemCellId,
                order.RewardGold,
                order.RewardExp
            );
        }

        /// <summary>
        /// 生成新订单
        /// </summary>
        public static OrderData GenerateOrder(int playerLevel, int randomSeed)
        {
            var random = new Random(randomSeed);
            
            // 根据玩家等级确定物品等级
            int itemLevel = Math.Min((playerLevel / 10) + 1, 10);
            
            // 随机选择物品
            int itemId = random.Next(1, 9);

            // 计算奖励
            int baseExp = 20 + playerLevel * 5;
            int baseGold = 10 + playerLevel * 3;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            return new OrderData
            {
                OrderId = Guid.NewGuid().GetHashCode(),
                RequireItemId = itemId,
                RewardExp = baseExp,
                RewardGold = baseGold,
                CreateTime = now,
                ExpireTime = now + ORDER_EXPIRE_HOURS * 3600,
                IsCompleted = false
            };
        }

        /// <summary>
        /// 查找订单
        /// </summary>
        private static OrderData FindOrder(GameState state, int orderId)
        {
            foreach (var order in state.Orders)
            {
                if (order.OrderId == orderId)
                {
                    return order;
                }
            }
            return null;
        }

        /// <summary>
        /// 在背包中查找物品
        /// </summary>
        private static int FindItemInBackpack(GameState state, int itemId)
        {
            for (int i = 0; i < 63; i++)
            {
                ref var cell = ref state.Cells[i];
                if (!cell.IsLocked && cell.HasItem && cell.ItemId == itemId)
                {
                    return i;
                }
            }
            return -1;
        }
    }

    /// <summary>
    /// 订单结果
    /// 不可变 纯数据
    /// </summary>
    public readonly struct OrderResult
    {
        public bool Success { get; }
        public int OrderId { get; }
        public int ConsumedCellId { get; }
        public int RewardGold { get; }
        public int RewardExp { get; }
        public string FailReason { get; }

        private OrderResult(bool success, int orderId, int consumedCellId, int rewardGold, int rewardExp, string failReason)
        {
            Success = success;
            OrderId = orderId;
            ConsumedCellId = consumedCellId;
            RewardGold = rewardGold;
            RewardExp = rewardExp;
            FailReason = failReason;
        }

        public static OrderResult Success(int orderId, int consumedCellId, int rewardGold, int rewardExp)
        {
            return new OrderResult(true, orderId, consumedCellId, rewardGold, rewardExp, null);
        }

        public static OrderResult Fail(string reason)
        {
            return new OrderResult(false, 0, -1, 0, 0, reason);
        }
    }

    /// <summary>
    /// 订单数据
    /// 不可变 纯数据
    /// </summary>
    public readonly struct OrderData
    {
        public int OrderId { get; }
        public int RequireItemId { get; }
        public int RewardExp { get; }
        public int RewardGold { get; }
        public long CreateTime { get; }
        public long ExpireTime { get; }
        public bool IsCompleted { get; }

        public OrderData(int orderId, int requireItemId, int rewardExp, int rewardGold, long createTime, long expireTime, bool isCompleted)
        {
            OrderId = orderId;
            RequireItemId = requireItemId;
            RewardExp = rewardExp;
            RewardGold = rewardGold;
            CreateTime = createTime;
            ExpireTime = expireTime;
            IsCompleted = isCompleted;
        }
    }
}
