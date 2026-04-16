using System;
using System.Collections.Generic;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.Order
{
    /// <summary>
    /// 订单系统
    /// 负责订单生成、交付、每日重置
    /// </summary>
    public class OrderSystem
    {
        private readonly EventBus _eventBus;
        private readonly StateMutator _stateMutator;
        private const int MAX_DAILY_ORDERS = 5;

        public OrderSystem(EventBus eventBus, StateMutator stateMutator)
        {
            _eventBus = eventBus;
            _stateMutator = stateMutator;
            
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            _eventBus.Listen<OrderSubmitRequestEvent>(OnOrderSubmitRequest);
            _eventBus.Listen<GameStartedEvent>(OnGameStarted);
        }

        private void OnGameStarted(GameStartedEvent e)
        {
            // 检查跨天重置
            CheckDailyReset();
            
            // 如果没有今天的订单则生成新订单
            if (!HasTodayOrders())
            {
                GenerateDailyOrders();
            }
        }

        private void OnOrderSubmitRequest(OrderSubmitRequestEvent e)
        {
            var state = _stateMutator.CurrentState;
            
            // 验证订单是否存在
            if (!state.Orders.ContainsKey(e.OrderId))
            {
                _eventBus.Publish(new OrderSubmitFailedEvent 
                { 
                    OrderId = e.OrderId, 
                    Reason = "订单不存在" 
                });
                return;
            }

            var order = state.Orders[e.OrderId];
            
            // 验证订单是否已完成
            if (order.IsCompleted)
            {
                _eventBus.Publish(new OrderSubmitFailedEvent 
                { 
                    OrderId = e.OrderId, 
                    Reason = "订单已完成" 
                });
                return;
            }

            // 验证物品是否足够
            if (!HasEnoughItems(order.RequiredItemId, order.RequiredCount))
            {
                _eventBus.Publish(new OrderSubmitFailedEvent 
                { 
                    OrderId = e.OrderId, 
                    Reason = "物品数量不足" 
                });
                return;
            }

            // 扣除物品
            RemoveItems(order.RequiredItemId, order.RequiredCount);

            // 发放奖励
            var newPlayer = _stateMutator.CurrentState.Player;
            newPlayer.Exp += order.ExpReward;
            newPlayer.Coins += order.CoinReward;

            // 标记订单完成
            var newOrders = new Dictionary<int, OrderData>(state.Orders);
            newOrders[e.OrderId] = order.WithCompleted(true);

            // 更新状态
            _stateMutator.UpdateState(state.Cells, newPlayer, newOrders);

            _eventBus.Publish(new OrderSubmittedEvent 
            { 
                OrderId = e.OrderId,
                ExpReward = order.ExpReward,
                CoinReward = order.CoinReward
            });
        }

        /// <summary>
        /// 检查是否需要每日重置
        /// </summary>
        public void CheckDailyReset()
        {
            var state = _stateMutator.CurrentState;
            var today = DateTime.UtcNow.Date;
            
            if (state.LastOrderResetDate < today)
            {
                // 跨天重置
                GenerateDailyOrders();
                
                _eventBus.Publish(new DailyResetEvent
                {
                    ResetDate = today,
                    ResetType = ResetType.Orders
                });
            }
        }

        /// <summary>
        /// 生成今日订单
        /// </summary>
        public void GenerateDailyOrders()
        {
            var orders = new Dictionary<int, OrderData>();
            var random = new Random();

            for (int i = 0; i < MAX_DAILY_ORDERS; i++)
            {
                var order = GenerateSingleOrder(i, random);
                orders[i] = order;
            }

            var state = _stateMutator.CurrentState;
            _stateMutator.UpdateState(
                cells: state.Cells,
                player: state.Player,
                orders: orders,
                lastOrderResetDate: DateTime.UtcNow.Date
            );

            _eventBus.Publish(new DailyOrdersGeneratedEvent
            {
                OrderCount = orders.Count
            });
        }

        private OrderData GenerateSingleOrder(int orderId, Random random)
        {
            // 根据玩家等级生成合适的订单
            int playerLevel = _stateMutator.CurrentState.Player.Level;
            
            // 物品ID范围: L1-L(等级+2)
            int maxItemLevel = Math.Min(playerLevel + 2, 10);
            int itemId = random.Next(1, maxItemLevel * 5);
            
            // 数量 1-3
            int count = random.Next(1, 4);
            
            // 奖励计算
            int expReward = itemId * count * 10;
            int coinReward = itemId * count * 5;

            return new OrderData
            {
                OrderId = orderId,
                RequiredItemId = itemId,
                RequiredCount = count,
                ExpReward = expReward,
                CoinReward = coinReward,
                IsCompleted = false,
                GeneratedTime = DateTime.UtcNow
            };
        }

        private bool HasEnoughItems(int itemId, int requiredCount)
        {
            int totalCount = 0;
            foreach (var cell in _stateMutator.CurrentState.Cells)
            {
                if (cell.ItemId == itemId)
                {
                    totalCount += cell.Count;
                }
            }
            return totalCount >= requiredCount;
        }

        private void RemoveItems(int itemId, int countToRemove)
        {
            var newCells = (CellState[])_stateMutator.CurrentState.Cells.Clone();
            int remaining = countToRemove;

            for (int i = 0; i < newCells.Length && remaining > 0; i++)
            {
                if (newCells[i].ItemId == itemId)
                {
                    if (newCells[i].Count <= remaining)
                    {
                        remaining -= newCells[i].Count;
                        newCells[i] = CellState.Empty(i);
                    }
                    else
                    {
                        newCells[i] = CellState.Create(i, itemId, newCells[i].Count - remaining);
                        remaining = 0;
                    }
                }
            }

            var state = _stateMutator.CurrentState;
            _stateMutator.UpdateState(newCells, state.Player, state.Orders);
        }

        private bool HasTodayOrders()
        {
            var state = _stateMutator.CurrentState;
            return state.Orders != null && 
                   state.Orders.Count > 0 && 
                   state.LastOrderResetDate == DateTime.UtcNow.Date;
        }
    }

    #region 订单数据结构

    [Serializable]
    public struct OrderData
    {
        public int OrderId;
        public int RequiredItemId;
        public int RequiredCount;
        public int ExpReward;
        public int CoinReward;
        public bool IsCompleted;
        public DateTime GeneratedTime;

        public OrderData WithCompleted(bool completed)
        {
            var copy = this;
            copy.IsCompleted = completed;
            return copy;
        }
    }

    #endregion

    #region 事件定义

    public class OrderSubmitRequestEvent : GameEvent
    {
        public int OrderId;
    }

    public class OrderSubmittedEvent : GameEvent
    {
        public int OrderId;
        public int ExpReward;
        public int CoinReward;
    }

    public class OrderSubmitFailedEvent : GameEvent
    {
        public int OrderId;
        public string Reason;
    }

    public class DailyOrdersGeneratedEvent : GameEvent
    {
        public int OrderCount;
    }

    public class DailyResetEvent : GameEvent
    {
        public DateTime ResetDate;
        public ResetType ResetType;
    }

    public enum ResetType
    {
        Orders,
        Missions,
        All
    }

    public class GameStartedEvent : GameEvent
    {
    }

    public class GameStateLoadedEvent : GameEvent
    {
        public GameState LoadedState;
    }

    #endregion
}