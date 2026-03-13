using System.Collections.Generic;
using TestWebGL.Game.Items;
using TestWebGL.Game.Grid;
using TestWebGL.Game.Player;

namespace TestWebGL.Game.Order
{
    /// <summary>
    /// 订单引擎 - 执行订单相关的游戏逻辑
    /// 与CraftingEngine类似，负责订单的提交和奖励分发
    /// </summary>
    public class OrderEngine
    {
        private OrderSystem _orderSystem;
        private GridManager _gridManager;
        private PlayerManager _playerManager;

        // 事件
        public delegate void OrderSubmitResultHandler(bool success, OrderSystem.OrderData order, string message);
        public event OrderSubmitResultHandler OnOrderSubmitResult;

        public delegate void OrderRewardGrantedHandler(OrderSystem.RewardType rewardType, int amount);
        public event OrderRewardGrantedHandler OnRewardGranted;

        public OrderEngine(OrderSystem orderSystem, GridManager gridManager, PlayerManager playerManager)
        {
            _orderSystem = orderSystem;
            _gridManager = gridManager;
            _playerManager = playerManager;
        }

        /// <summary>
        /// 尝试提交订单
        /// </summary>
        public bool TrySubmitOrder(int orderId)
        {
            // 1. 检查订单是否存在
            var orders = _orderSystem.GetCurrentOrders();
            OrderSystem.OrderData targetOrder = default;
            bool found = false;

            foreach (var order in orders)
            {
                if (order.orderId == orderId && !order.isCompleted)
                {
                    targetOrder = order;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                OnOrderSubmitResult?.Invoke(false, targetOrder, "订单未找到或已完成");
                UnityEngine.Debug.Log($"[OrderEngine] 订单提交失败：订单未找到");
                return false;
            }

            // 2. 验证背包中是否有足够的物品
            if (!HasRequiredItems(targetOrder))
            {
                OnOrderSubmitResult?.Invoke(false, targetOrder, "背包物品不足");
                UnityEngine.Debug.Log($"[OrderEngine] 订单提交失败：物品不足");
                return false;
            }

            // 3. 从网格中消耗物品
            if (!TryConsumeItems(targetOrder))
            {
                OnOrderSubmitResult?.Invoke(false, targetOrder, "物品消耗失败");
                UnityEngine.Debug.Log($"[OrderEngine] 订单提交失败：物品消耗失败");
                return false;
            }

            // 4. 完成订单
            _orderSystem.TryCompleteOrder(orderId, _gridManager);

            // 5. 分发奖励
            GrantOrderReward(targetOrder);

            OnOrderSubmitResult?.Invoke(true, targetOrder, "订单完成成功");
            UnityEngine.Debug.Log($"[OrderEngine] 订单提交成功：{targetOrder.GetDisplayName()}");
            return true;
        }

        /// <summary>
        /// 检查背包中是否有足够的物品
        /// </summary>
        private bool HasRequiredItems(OrderSystem.OrderData order)
        {
            int itemCount = 0;
            var cells = _gridManager.GetAllCells();

            foreach (var cell in cells)
            {
                if (!cell.IsLocked && cell.HasItem)
                {
                    if (cell.CurrentItemType == order.requiredItem && 
                        ItemConfig.GetItemLevel(cell.CurrentItemType) == order.requiredLevel)
                    {
                        itemCount += cell.ItemCount;
                    }
                }
            }

            UnityEngine.Debug.Log($"[OrderEngine] 检查物品: 需要{order.requiredCount}个, 有{itemCount}个");
            return itemCount >= order.requiredCount;
        }

        /// <summary>
        /// 从网格中消耗物品
        /// </summary>
        private bool TryConsumeItems(OrderSystem.OrderData order)
        {
            int remaining = order.requiredCount;
            var cells = _gridManager.GetAllCells();

            foreach (var cell in cells)
            {
                if (remaining == 0)
                    break;

                if (!cell.IsLocked && cell.HasItem &&
                    cell.CurrentItemType == order.requiredItem && 
                    ItemConfig.GetItemLevel(cell.CurrentItemType) == order.requiredLevel)
                {
                    int toRemove = System.Math.Min(remaining, cell.ItemCount);
                    
                    // 从网格中移除物品
                    var (removeSuccess, removeError) = _gridManager.TryRemoveItem(cell.row, cell.column, toRemove);
                    if (removeSuccess)
                    {
                        remaining -= toRemove;
                        UnityEngine.Debug.Log($"[OrderEngine] 消耗 {ItemConfig.GetItemName(order.requiredItem)} ×{toRemove}");
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"[OrderEngine] 移除物品失败 [{cell.row},{cell.column}]: {removeError}");
                    }
                }
            }

            return remaining == 0;
        }

        /// <summary>
        /// 分发订单奖励
        /// </summary>
        private void GrantOrderReward(OrderSystem.OrderData order)
        {
            UnityEngine.Debug.Log($"[OrderEngine] 分发订单奖励: {order.rewardType} ×{order.rewardAmount}");

            switch (order.rewardType)
            {
                case OrderSystem.RewardType.EnergyDrink:
                    // 临时增加体力（1小时内有效）
                    // 暂时实现为直接增加体力
                    _playerManager.RecoverStamina(order.rewardAmount);
                    UnityEngine.Debug.Log($"[OrderEngine] 获得能量饮料: +{order.rewardAmount} 体力（1小时有效）");
                    break;

                case OrderSystem.RewardType.StaminaNormal:
                    _playerManager.RecoverStamina(order.rewardAmount);
                    UnityEngine.Debug.Log($"[OrderEngine] 获得体力: +{order.rewardAmount}");
                    break;

                case OrderSystem.RewardType.StaminaLarge:
                    _playerManager.RecoverStamina(order.rewardAmount);
                    UnityEngine.Debug.Log($"[OrderEngine] 获得大量体力: +{order.rewardAmount}");
                    break;

                case OrderSystem.RewardType.Experience:
                    _playerManager.GainExperience(order.rewardAmount, "订单完成");
                    UnityEngine.Debug.Log($"[OrderEngine] 获得经验: +{order.rewardAmount}");
                    break;

                case OrderSystem.RewardType.Title:
                    // 获得称号（后期UI展示）
                    UnityEngine.Debug.Log($"[OrderEngine] 获得称号: {order.rewardAmount}");
                    break;

                case OrderSystem.RewardType.Unlock:
                    // 解锁新生产线（后期实现）
                    UnityEngine.Debug.Log($"[OrderEngine] 解锁新生产线");
                    break;
            }

            // 同时给予基础经验（根据订单等级）
            int baseExpReward = _orderSystem.CalculateOrderRewardExperience(order);
            if (baseExpReward > 0)
            {
                _playerManager.GainExperience(baseExpReward, "订单完成奖励");
                UnityEngine.Debug.Log($"[OrderEngine] 获得订单奖励经验: +{baseExpReward}");
            }

            OnRewardGranted?.Invoke(order.rewardType, order.rewardAmount);
        }

        /// <summary>
        /// 获取订单完成进度
        /// 返回(已完成数, 总数)
        /// </summary>
        public (int completed, int total) GetOrderProgress()
        {
            var orders = _orderSystem.GetCurrentOrders();
            int completed = 0;

            foreach (var order in orders)
            {
                if (order.isCompleted)
                    completed++;
            }

            return (completed, orders.Length);
        }
    }
}
