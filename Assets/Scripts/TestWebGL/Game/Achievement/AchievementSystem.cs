using System;
using System.Collections.Generic;

namespace TestWebGL.Game.Achievement
{
    /// <summary>
    /// 成就类型枚举
    /// </summary>
    public enum AchievementType
    {
        FirstExplore,           // 首次探索
        GridMaster,            // 填满网格
        LevelUp,               // 升级
        CraftingMaster,        // 合成大师
        OrderComplete,         // 完成订单
        CollectionComplete,    // 收集所有物品
        SpeedRunner,           // 快速通关
        Persistence,           // 坚持不懈
    }

    /// <summary>
    /// 成就数据结构
    /// </summary>
    [Serializable]
    public struct AchievementData
    {
        public AchievementType type;
        public string title;
        public string description;
        public bool isUnlocked;
        public DateTime unlockTime;
        public int progress;        // 当前进度
        public int targetProgress;  // 目标进度

        public AchievementData(AchievementType type, string title, string description, int targetProgress = 1)
        {
            this.type = type;
            this.title = title;
            this.description = description;
            this.isUnlocked = false;
            this.unlockTime = DateTime.MinValue;
            this.progress = 0;
            this.targetProgress = targetProgress;
        }
    }

    /// <summary>
    /// 成就系统
    /// 管理所有成就的解锁和进度跟踪
    /// </summary>
    public class AchievementSystem
    {
        private static AchievementSystem s_instance;
        public static AchievementSystem Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new AchievementSystem();
                }
                return s_instance;
            }
        }

        // 成就数据库
        private Dictionary<AchievementType, AchievementData> _achievements;

        /// <summary>
        /// 成就解锁事件
        /// </summary>
        public event Action<AchievementData> OnAchievementUnlocked;

        private AchievementSystem()
        {
            InitializeAchievements();
        }

        /// <summary>
        /// 初始化成就数据库
        /// </summary>
        private void InitializeAchievements()
        {
            _achievements = new Dictionary<AchievementType, AchievementData>
            {
                { AchievementType.FirstExplore, new AchievementData(
                    AchievementType.FirstExplore,
                    "初次探索",
                    "进行第一次探索",
                    1) },

                { AchievementType.GridMaster, new AchievementData(
                    AchievementType.GridMaster,
                    "网格大师",
                    "将9×7网格全部填满",
                    63) }, // 9*7=63

                { AchievementType.LevelUp, new AchievementData(
                    AchievementType.LevelUp,
                    "等级提升",
                    "玩家等级达到5级",
                    5) },

                { AchievementType.CraftingMaster, new AchievementData(
                    AchievementType.CraftingMaster,
                    "合成大师",
                    "成功进行10次合成",
                    10) },

                { AchievementType.OrderComplete, new AchievementData(
                    AchievementType.OrderComplete,
                    "订单达人",
                    "完成5个订单",
                    5) },

                { AchievementType.CollectionComplete, new AchievementData(
                    AchievementType.CollectionComplete,
                    "收藏家",
                    "收集所有9种L1物品",
                    9) },

                { AchievementType.SpeedRunner, new AchievementData(
                    AchievementType.SpeedRunner,
                    "速度狂人",
                    "在10分钟内达到等级3",
                    1) }, // 特殊成就，需要时间检查

                { AchievementType.Persistence, new AchievementData(
                    AchievementType.Persistence,
                    "坚持不懈",
                    "连续玩游戏7天",
                    7) }, // 天数
            };

            Debug.Log($"[AchievementSystem] 初始化了 {_achievements.Count} 个成就");
        }

        /// <summary>
        /// 更新成就进度
        /// </summary>
        public void UpdateProgress(AchievementType type, int progressIncrement = 1)
        {
            if (_achievements.TryGetValue(type, out var achievement))
            {
                if (achievement.isUnlocked) return; // 已解锁的成就不再更新

                achievement.progress += progressIncrement;

                // 检查是否达到解锁条件
                if (achievement.progress >= achievement.targetProgress)
                {
                    UnlockAchievement(type);
                }
                else
                {
                    // 更新成就数据
                    _achievements[type] = achievement;
                }
            }
        }

        /// <summary>
        /// 直接设置成就进度（用于特殊成就）
        /// </summary>
        public void SetProgress(AchievementType type, int progress)
        {
            if (_achievements.TryGetValue(type, out var achievement))
            {
                if (achievement.isUnlocked) return;

                achievement.progress = progress;

                if (achievement.progress >= achievement.targetProgress)
                {
                    UnlockAchievement(type);
                }
                else
                {
                    _achievements[type] = achievement;
                }
            }
        }

        /// <summary>
        /// 解锁成就
        /// </summary>
        private void UnlockAchievement(AchievementType type)
        {
            if (_achievements.TryGetValue(type, out var achievement))
            {
                achievement.isUnlocked = true;
                achievement.unlockTime = DateTime.Now;
                _achievements[type] = achievement;

                Debug.Log($"[AchievementSystem] 成就解锁: {achievement.title}");

                // 触发事件
                OnAchievementUnlocked?.Invoke(achievement);
            }
        }

        /// <summary>
        /// 获取成就数据
        /// </summary>
        public AchievementData GetAchievement(AchievementType type)
        {
            return _achievements.TryGetValue(type, out var achievement) ? achievement : default;
        }

        /// <summary>
        /// 获取所有成就
        /// </summary>
        public Dictionary<AchievementType, AchievementData> GetAllAchievements()
        {
            return new Dictionary<AchievementType, AchievementData>(_achievements);
        }

        /// <summary>
        /// 获取已解锁成就数量
        /// </summary>
        public int GetUnlockedCount()
        {
            int count = 0;
            foreach (var achievement in _achievements.Values)
            {
                if (achievement.isUnlocked) count++;
            }
            return count;
        }

        /// <summary>
        /// 获取总成就数量
        /// </summary>
        public int GetTotalCount()
        {
            return _achievements.Count;
        }

        /// <summary>
        /// 检查成就是否已解锁
        /// </summary>
        public bool IsUnlocked(AchievementType type)
        {
            return _achievements.TryGetValue(type, out var achievement) && achievement.isUnlocked;
        }

        /// <summary>
        /// 重置所有成就（用于调试）
        /// </summary>
        public void ResetAllAchievements()
        {
            foreach (var type in _achievements.Keys)
            {
                var achievement = _achievements[type];
                achievement.isUnlocked = false;
                achievement.unlockTime = DateTime.MinValue;
                achievement.progress = 0;
                _achievements[type] = achievement;
            }
            Debug.Log("[AchievementSystem] 所有成就已重置");
        }
    }
}