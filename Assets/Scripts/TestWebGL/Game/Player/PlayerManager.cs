using System;
using System.Collections.Generic;
using TestWebGL.Game.Items;

namespace TestWebGL.Game.Player
{
    /// <summary>
    /// 玩家存档数据
    /// 包含所有需要持久化的玩家信息
    /// </summary>
    [System.Serializable]
    public class PlayerData
    {
        // 基础信息
        public int playerId;
        public string playerName = "玩家";
        public DateTime createdTime;
        public DateTime lastSaveTime;

        // 等级系统
        public int level = 1;
        public int experience = 0;

        // 体力系统
        public int currentStamina = 20;
        public int maxStamina = 20;
        public DateTime lastStaminaRecoverTime;

        // 历史记录
        public int historyMaxLevel = 1;

        // 图鉴集合（记录已合成过的物品）
        public List<int> collectedItems = new List<int>();

        // 成就、称号等（预留）
        public List<int> unlockedAchievements = new List<int>();

        // 时间戳记录
        public int dailyOrderRefreshCount = 0;
        public DateTime lastOrderRefreshTime;

        public PlayerData()
        {
            createdTime = DateTime.Now;
            lastSaveTime = DateTime.Now;
            lastStaminaRecoverTime = DateTime.Now;
            lastOrderRefreshTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 玩家管理器
    /// 管理玩家的等级、经验、体力等系统
    /// </summary>
    public class PlayerManager
    {
        private PlayerData _playerData;

        // 等级经验配置（来自设计规范6.2）
        private readonly int[] _levelExpRequirement = new int[]
        {
            0,      // Lv0 (无效)
            100,    // Lv1
            200,    // Lv2
            300,    // Lv3
            400,    // Lv4
            500,    // Lv5
            600,    // Lv6
            700,    // Lv7
            800,    // Lv8
            900,    // Lv9
            1000,   // Lv10
            1100,   // Lv11
            1200,   // Lv12
            1300,   // Lv13
            1400,   // Lv14
            1500,   // Lv15
            1600,   // Lv16
            1700,   // Lv17
            1800,   // Lv18
            1900,   // Lv19
            2000,   // Lv20
            // 后续等级按照 100 × 等级 的规律递增
            // 这里设置到Lv100作为示例，实际可以无限
        };

        // 体力上限成长表（来自设计规范6.3）
        private readonly (int level, int maxStamina, int recoveryMinutes)[] _staminaGrowthTable = new[]
        {
            (1, 20, 10),
            (10, 25, 10),
            (20, 35, 9),
            (30, 55, 8),
            (40, 85, 7),
            (50, 125, 6),
            (60, 175, 5),
            (70, 235, 4),
            (80, 315, 4),
            (90, 405, 3),
            (100, 505, 3),
        };

        // 事件定义
        public delegate void LevelChangedHandler(int newLevel, int oldLevel);
        public event LevelChangedHandler OnLevelChanged;

        public delegate void StaminaChangedHandler(int newStamina, int maxStamina);
        public event StaminaChangedHandler OnStaminaChanged;

        public delegate void ExperienceGainedHandler(int amount, string reason);
        public event ExperienceGainedHandler OnExperienceGained;

        /// <summary>
        /// 初始化玩家管理器
        /// </summary>
        public void Initialize(PlayerData playerData = null)
        {
            if (playerData != null)
                _playerData = playerData;
            else
                _playerData = new PlayerData();
        }

        /// <summary>
        /// 获取玩家数据（拷贝，避免外部直接修改）
        /// </summary>
        public PlayerData GetPlayerData()
        {
            return _playerData;
        }

        /// <summary>
        /// 增加经验值
        /// </summary>
        public void GainExperience(int amount, string reason = "")
        {
            if (amount <= 0)
                return;

            int oldLevel = _playerData.level;
            _playerData.experience += amount;

            // 检查是否升级
            while (CanLevelUp())
            {
                LevelUp();
            }

            OnExperienceGained?.Invoke(amount, reason);

            if (oldLevel != _playerData.level)
            {
                int newLevel = _playerData.level;
                if (newLevel > _playerData.historyMaxLevel)
                    _playerData.historyMaxLevel = newLevel;
                OnLevelChanged?.Invoke(newLevel, oldLevel);
            }
        }

        /// <summary>
        /// 检查是否可以升级
        /// </summary>
        private bool CanLevelUp()
        {
            int nextLevelExp = GetExperienceForLevel(_playerData.level + 1);
            return _playerData.experience >= nextLevelExp;
        }

        /// <summary>
        /// 升级
        /// </summary>
        private void LevelUp()
        {
            _playerData.level++;

            // 更新体力上限
            UpdateMaxStamina();

            // 恢复满体力
            _playerData.currentStamina = _playerData.maxStamina;
            OnStaminaChanged?.Invoke(_playerData.currentStamina, _playerData.maxStamina);
        }

        /// <summary>
        /// 获取指定等级需要的经验值（从0起）
        /// </summary>
        public int GetExperienceForLevel(int level)
        {
            if (level <= 1)
                return 0;

            int totalExp = 0;
            for (int i = 1; i < level; i++)
            {
                int levelExp = Mathf.Min(i, _levelExpRequirement.Length - 1);
                totalExp += _levelExpRequirement[levelExp];
            }
            return totalExp;
        }

        /// <summary>
        /// 获取升级所需的剩余经验
        /// </summary>
        public int GetRemainingExpForLevelUp()
        {
            int nextLevelExp = GetExperienceForLevel(_playerData.level + 1);
            return nextLevelExp - _playerData.experience;
        }

        /// <summary>
        /// 更新体力上限（根据等级）
        /// </summary>
        private void UpdateMaxStamina()
        {
            int newMaxStamina = 20;

            foreach (var entry in _staminaGrowthTable)
            {
                if (_playerData.level >= entry.level)
                    newMaxStamina = entry.maxStamina;
                else
                    break;
            }

            if (newMaxStamina != _playerData.maxStamina)
            {
                _playerData.maxStamina = newMaxStamina;
                OnStaminaChanged?.Invoke(_playerData.currentStamina, _playerData.maxStamina);
            }
        }

        /// <summary>
        /// 消耗体力
        /// </summary>
        public bool TryUseStamina(int amount)
        {
            if (_playerData.currentStamina < amount)
                return false;

            _playerData.currentStamina -= amount;
            OnStaminaChanged?.Invoke(_playerData.currentStamina, _playerData.maxStamina);
            return true;
        }

        /// <summary>
        /// 消耗体力（不检查，直接扣）
        /// </summary>
        public void UseStamina(int amount)
        {
            TryUseStamina(amount);
        }

        /// <summary>
        /// 获取当前体力
        /// </summary>
        public int GetCurrentStamina()
        {
            return _playerData.currentStamina;
        }

        /// <summary>
        /// 获取玩家等级
        /// </summary>
        public int GetLevel()
        {
            return _playerData.level;
        }

        /// <summary>
        /// 恢复体力
        /// </summary>
        public void RecoverStamina(int amount)
        {
            _playerData.currentStamina = Mathf.Min(_playerData.currentStamina + amount, _playerData.maxStamina);
            OnStaminaChanged?.Invoke(_playerData.currentStamina, _playerData.maxStamina);
        }

        /// <summary>
        /// 获取体力恢复速度（分钟）
        /// </summary>
        public int GetStaminaRecoveryMinutes()
        {
            foreach (var entry in _staminaGrowthTable)
            {
                if (_playerData.level >= entry.level)
                    return entry.recoveryMinutes;
            }
            return 10;
        }

        /// <summary>
        /// 计算自动恢复的体力量（基于离线时间）
        /// </summary>
        public int CalculateAutoRecoveredStamina()
        {
            TimeSpan elapsed = DateTime.Now - _playerData.lastStaminaRecoverTime;
            int recoveryMinutes = GetStaminaRecoveryMinutes();
            int recoveredPoints = (int)(elapsed.TotalMinutes / recoveryMinutes);

            return recoveredPoints;
        }

        /// <summary>
        /// 记录物品已合成（用于图鉴）
        /// </summary>
        public void RecordItemCollected(ItemType itemType)
        {
            int itemId = (int)itemType;
            if (!_playerData.collectedItems.Contains(itemId))
                _playerData.collectedItems.Add(itemId);
        }

        /// <summary>
        /// 检查物品是否已合成过
        /// </summary>
        public bool IsItemCollected(ItemType itemType)
        {
            return _playerData.collectedItems.Contains((int)itemType);
        }

        /// <summary>
        /// 更新最后保存时间
        /// </summary>
        public void UpdateLastSaveTime()
        {
            _playerData.lastSaveTime = DateTime.Now;
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public PlayerStatistics GetStatistics()
        {
            return new PlayerStatistics
            {
                level = _playerData.level,
                experience = _playerData.experience,
                currentStamina = _playerData.currentStamina,
                maxStamina = _playerData.maxStamina,
                collectedItemCount = _playerData.collectedItems.Count,
                playTime = DateTime.Now - _playerData.createdTime,
            };
        }

        /// <summary>
        /// 获取历史最高等级
        /// </summary>
        public int GetHistoryMaxLevel()
        {
            return _playerData.historyMaxLevel;
        }
    }

    /// <summary>
    /// 玩家统计信息
    /// </summary>
    [System.Serializable]
    public class PlayerStatistics
    {
        public int level;
        public int experience;
        public int currentStamina;
        public int maxStamina;
        public int collectedItemCount;
        public TimeSpan playTime;
    }

    /// <summary>
    /// 简单数学工具类（Unity中通常用Mathf，这里为了避免依赖做了简化）
    /// </summary>
    public static class Mathf
    {
        public static int Min(int a, int b) => a < b ? a : b;
        public static int Max(int a, int b) => a > b ? a : b;
        public static float Min(float a, float b) => a < b ? a : b;
        public static float Max(float a, float b) => a > b ? a : b;
    }
}
