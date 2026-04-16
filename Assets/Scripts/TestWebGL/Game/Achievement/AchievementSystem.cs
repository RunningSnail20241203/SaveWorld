using System;
using System.Collections.Generic;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.Achievement
{
    /// <summary>
    /// 成就系统
    /// 负责成就进度跟踪、解锁判定、奖励发放
    /// </summary>
    public class AchievementSystem
    {
        private readonly EventBus _eventBus;
        private readonly StateMutator _stateMutator;

        public AchievementSystem(EventBus eventBus, StateMutator stateMutator)
        {
            _eventBus = eventBus;
            _stateMutator = stateMutator;
            
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            // 监听所有游戏事件用于成就判定
            _eventBus.Listen<MergeCompleteEvent>(OnMergeComplete);
            _eventBus.Listen<ExplorationCompleteEvent>(OnExplorationComplete);
            _eventBus.Listen<OrderSubmittedEvent>(OnOrderSubmitted);
            _eventBus.Listen<LevelUpEvent>(OnLevelUp);
            _eventBus.Listen<AchievementUnlockRequestEvent>(OnAchievementUnlockRequest);
        }

        private void OnMergeComplete(MergeCompleteEvent e)
        {
            UpdateAchievementProgress(AchievementType.TotalMerges, 1);
            UpdateAchievementProgress(AchievementType.ItemLevelUp, e.NewItemId / 5);
        }

        private void OnExplorationComplete(ExplorationCompleteEvent e)
        {
            UpdateAchievementProgress(AchievementType.TotalExplorations, 1);
            UpdateAchievementProgress(AchievementType.ItemsCollected, e.GeneratedCellIds.Length);
        }

        private void OnOrderSubmitted(OrderSubmittedEvent e)
        {
            UpdateAchievementProgress(AchievementType.OrdersCompleted, 1);
        }

        private void OnLevelUp(LevelUpEvent e)
        {
            UpdateAchievementProgress(AchievementType.PlayerLevel, e.NewLevel);
        }

        private void OnAchievementUnlockRequest(AchievementUnlockRequestEvent e)
        {
            if (IsAchievementUnlocked(e.AchievementId))
            {
                _eventBus.Publish(new AchievementUnlockFailedEvent
                {
                    AchievementId = e.AchievementId,
                    Reason = "成就已经解锁"
                });
                return;
            }

            if (!CanUnlockAchievement(e.AchievementId))
            {
                _eventBus.Publish(new AchievementUnlockFailedEvent
                {
                    AchievementId = e.AchievementId,
                    Reason = "成就条件未达成"
                });
                return;
            }

            UnlockAchievement(e.AchievementId);
        }

        /// <summary>
        /// 更新成就进度
        /// </summary>
        private void UpdateAchievementProgress(AchievementType type, int value)
        {
            var state = _stateMutator.CurrentState;
            
            if (!state.Achievements.ContainsKey((int)type))
            {
                return;
            }

            var achievement = state.Achievements[(int)type];
            
            if (achievement.IsUnlocked)
            {
                return;
            }

            achievement.Progress += value;

            // 检查是否达成条件
            if (achievement.Progress >= achievement.RequiredProgress)
            {
                achievement.Progress = achievement.RequiredProgress;
                achievement.IsUnlocked = true;
                achievement.UnlockTime = DateTime.UtcNow;

                // 发放奖励
                var newPlayer = state.Player;
                newPlayer.Exp += achievement.ExpReward;
                newPlayer.Gold += achievement.CoinReward;

                var newAchievements = new Dictionary<int, AchievementData>(state.Achievements);
                newAchievements[(int)type] = achievement;

                _stateMutator.UpdateState(state.Cells, newPlayer, state.Orders, newAchievements);

                _eventBus.Publish(new AchievementUnlockedEvent
                {
                    AchievementId = (int)type,
                    AchievementName = achievement.Name,
                    ExpReward = achievement.ExpReward,
                    CoinReward = achievement.CoinReward
                });
            }
            else
            {
                var newAchievements = new Dictionary<int, AchievementData>(state.Achievements);
                newAchievements[(int)type] = achievement;
                
                _stateMutator.UpdateState(state.Cells, state.Player, state.Orders, newAchievements);
            }
        }

        private bool IsAchievementUnlocked(int achievementId)
        {
            var state = _stateMutator.CurrentState;
            return state.Achievements.ContainsKey(achievementId) && 
                   state.Achievements[achievementId].IsUnlocked;
        }

        private bool CanUnlockAchievement(int achievementId)
        {
            var state = _stateMutator.CurrentState;
            if (!state.Achievements.ContainsKey(achievementId))
            {
                return false;
            }

            var achievement = state.Achievements[achievementId];
            return achievement.Progress >= achievement.RequiredProgress && !achievement.IsUnlocked;
        }

        private void UnlockAchievement(int achievementId)
        {
            var state = _stateMutator.CurrentState;
            var achievement = state.Achievements[achievementId];
            
            achievement.IsUnlocked = true;
            achievement.UnlockTime = DateTime.UtcNow;

            var newPlayer = state.Player;
            newPlayer.Exp += achievement.ExpReward;
            newPlayer.Gold += achievement.CoinReward;

            var newAchievements = new Dictionary<int, AchievementData>(state.Achievements);
            newAchievements[achievementId] = achievement;

            _stateMutator.UpdateState(state.Cells, newPlayer, state.Orders, newAchievements);

            _eventBus.Publish(new AchievementUnlockedEvent
            {
                AchievementId = achievementId,
                AchievementName = achievement.Name,
                ExpReward = achievement.ExpReward,
                CoinReward = achievement.CoinReward
            });
        }

        /// <summary>
        /// 初始化所有成就配置
        /// </summary>
        public void InitializeAchievements()
        {
            var achievements = new Dictionary<int, AchievementData>();

            // 合成类成就
            AddAchievement(achievements, AchievementType.TotalMerges, "合成大师", "累计合成100次物品", 100, 500, 1000);
            AddAchievement(achievements, AchievementType.ItemLevelUp, "升级达人", "合成出10级物品", 10, 1000, 5000);
            
            // 探索类成就
            AddAchievement(achievements, AchievementType.TotalExplorations, "探险家", "累计探索100次", 100, 300, 500);
            AddAchievement(achievements, AchievementType.ItemsCollected, "收藏家", "收集500个物品", 500, 800, 2000);
            
            // 订单类成就
            AddAchievement(achievements, AchievementType.OrdersCompleted, "配送专家", "完成100个订单", 100, 600, 3000);
            
            // 等级类成就
            AddAchievement(achievements, AchievementType.PlayerLevel, "满级大佬", "达到100级", 100, 5000, 10000);

            var state = _stateMutator.CurrentState;
            _stateMutator.UpdateState(state.Cells, state.Player, state.Orders, achievements);
        }

        private void AddAchievement(Dictionary<int, AchievementData> achievements, AchievementType type, 
                                    string name, string description, int required, int exp, int coins)
        {
            achievements[(int)type] = new AchievementData
            {
                Id = (int)type,
                Name = name,
                Description = description,
                RequiredProgress = required,
                Progress = 0,
                ExpReward = exp,
                CoinReward = coins,
                IsUnlocked = false
            };
        }
    }

    #region 成就数据结构

    [Serializable]
    public struct AchievementData
    {
        public int Id;
        public string Name;
        public string Description;
        public int RequiredProgress;
        public int Progress;
        public int ExpReward;
        public int CoinReward;
        public bool IsUnlocked;
        public DateTime UnlockTime;
    }

    public enum AchievementType
    {
        TotalMerges = 1,
        ItemLevelUp = 2,
        TotalExplorations = 3,
        ItemsCollected = 4,
        OrdersCompleted = 5,
        PlayerLevel = 6
    }

    #endregion

    #region 事件定义

    public class AchievementUnlockRequestEvent : GameEvent
    {
        public int AchievementId;
    }

    public class AchievementUnlockedEvent : GameEvent
    {
        public int AchievementId;
        public string AchievementName;
        public int ExpReward;
        public int CoinReward;
    }

    public class AchievementUnlockFailedEvent : GameEvent
    {
        public int AchievementId;
        public string Reason;
    }

    public class AchievementProgressUpdatedEvent : GameEvent
    {
        public int AchievementId;
        public int Progress;
        public int RequiredProgress;
    }

    #endregion
}