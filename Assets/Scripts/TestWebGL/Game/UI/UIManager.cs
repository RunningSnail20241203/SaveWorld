using System;
using UnityEngine;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// UI管理器
    /// 监听游戏事件 响应状态变化 更新UI
    /// </summary>
    public class UIManager
    {
        private readonly EventBus _eventBus;
        private readonly StateMutator _stateMutator;

        public UIManager(EventBus eventBus, StateMutator stateMutator)
        {
            _eventBus = eventBus;
            _stateMutator = stateMutator;
            
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            // 监听所有游戏事件更新UI
            _eventBus.Listen<MergeCompleteEvent>(_ => RefreshGridUI());
            _eventBus.Listen<ItemMovedEvent>(_ => RefreshGridUI());
            _eventBus.Listen<ExplorationCompleteEvent>(_ => 
            {
                RefreshGridUI();
                RefreshPlayerUI();
            });
            _eventBus.Listen<OrderSubmittedEvent>(_ => 
            {
                RefreshGridUI();
                RefreshPlayerUI();
                RefreshOrdersUI();
            });
            _eventBus.Listen<AchievementUnlockedEvent>(e => ShowAchievementPopup(e));
            _eventBus.Listen<LevelUpEvent>(e => ShowLevelUpPopup(e));
            _eventBus.Listen<StaminaClaimedEvent>(_ => RefreshPlayerUI());
            _eventBus.Listen<CloudSyncStartedEvent>(_ => ShowSyncIndicator());
            _eventBus.Listen<CloudSyncCompletedEvent>(_ => HideSyncIndicator());
            _eventBus.Listen<SFXPlayedEvent>(_ => PlayButtonAnimation());
        }

        /// <summary>
        /// 刷新格子UI
        /// </summary>
        public void RefreshGridUI()
        {
            var state = _stateMutator.CurrentState;
            
            for (int i = 0; i < state.Cells.Length; i++)
            {
                UpdateCellUI(i, state.Cells[i]);
            }
            
            _eventBus.Publish(new GridUIRefreshedEvent());
        }

        /// <summary>
        /// 刷新玩家信息UI
        /// </summary>
        public void RefreshPlayerUI()
        {
            var state = _stateMutator.CurrentState;
            
            UpdatePlayerLevel(state.Player.Level);
            UpdatePlayerStamina(state.Player.Stamina, state.Player.MaxStamina);
            UpdatePlayerGold(state.Player.Gold);
            UpdatePlayerExp(state.Player.Exp, state.Player.ExpToNextLevel);
            
            _eventBus.Publish(new PlayerUIRefreshedEvent());
        }

        /// <summary>
        /// 刷新订单UI
        /// </summary>
        public void RefreshOrdersUI()
        {
            var state = _stateMutator.CurrentState;
            
            foreach (var order in state.Orders)
            {
                UpdateOrderUI(order.Key, order.Value);
            }
            
            _eventBus.Publish(new OrdersUIRefreshedEvent());
        }

        /// <summary>
        /// 显示成就解锁弹窗
        /// </summary>
        public void ShowAchievementPopup(AchievementUnlockedEvent e)
        {
            _eventBus.Publish(new AchievementPopupShownEvent
            {
                AchievementId = e.AchievementId,
                AchievementName = e.AchievementName
            });
        }

        /// <summary>
        /// 显示升级弹窗
        /// </summary>
        public void ShowLevelUpPopup(LevelUpEvent e)
        {
            _eventBus.Publish(new LevelUpPopupShownEvent
            {
                NewLevel = e.NewLevel
            });
        }

        private void UpdateCellUI(int cellId, CellState cell)
        {
            // TODO: 实际Unity UI更新
        }

        private void UpdatePlayerLevel(int level)
        {
        }

        private void UpdatePlayerStamina(int current, int max)
        {
        }

        private void UpdatePlayerGold(long gold)
        {
        }

        private void UpdatePlayerExp(int current, int max)
        {
        }

        private void UpdateOrderUI(int orderId, SaveWorld.Game.Order.OrderData order)
        {
        }

        private void ShowSyncIndicator()
        {
        }

        private void HideSyncIndicator()
        {
        }

        private void PlayButtonAnimation()
        {
        }
    }

    #region UI事件定义

    public class GridUIRefreshedEvent : GameEvent
    {
    }

    public class PlayerUIRefreshedEvent : GameEvent
    {
    }

    public class OrdersUIRefreshedEvent : GameEvent
    {
    }

    public class AchievementPopupShownEvent : GameEvent
    {
        public int AchievementId;
        public string AchievementName;
    }

    public class LevelUpPopupShownEvent : GameEvent
    {
        public int NewLevel;
    }

    #endregion
}