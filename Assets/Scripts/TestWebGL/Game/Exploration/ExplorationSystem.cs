using System;
using System.Collections.Generic;
using SaveWorld.Game.Items;

namespace SaveWorld.Game.Exploration
{
    /// <summary>
    /// 探索系统 - 管理玩家的探索行为和获得物品
    /// 设计规范第四节
    /// </summary>
    public class ExplorationSystem
    {
        // 随机数生成器
        private Random _random = new Random();

        // 事件
        public delegate void ExploreSuccessHandler(ItemType[] items);
        public event ExploreSuccessHandler OnExploreSuccess;

        public delegate void ExploreFailureHandler(string reason);
        public event ExploreFailureHandler OnExploreFailure;

        // 玩家等级段配置（设计规范4.2）
        private struct LevelBracket
        {
            public int minLevel;
            public int maxLevel;
            public ItemType[] generateableItems;  // 该等级段可生成的物品等级
        }

        private LevelBracket[] _levelBrackets;

        // L1物品池（设计规范4.3）
        private struct L1ItemPoolEntry
        {
            public ItemType itemType;
            public float probability;  // 0-1之间的概率
        }

        private L1ItemPoolEntry[] _l1ItemPool;

        public ExplorationSystem()
        {
            InitializeLevelBrackets();
            InitializeL1ItemPool();
        }

        /// <summary>
        /// 初始化等级段配置
        /// 每个等级段有不同的可生成物品等级范围
        /// </summary>
        private void InitializeLevelBrackets()
        {
            _levelBrackets = new LevelBracket[]
            {
                // Lv1-10：只生成L1
                new LevelBracket
                {
                    minLevel = 1,
                    maxLevel = 10,
                    generateableItems = new[] { ItemType.Water_L1, ItemType.Food_L1, ItemType.Tool_L1 }  // 只生成L1示例
                },
                // Lv11-20：生成L1、L2
                new LevelBracket
                {
                    minLevel = 11,
                    maxLevel = 20,
                    generateableItems = new[] { ItemType.Water_L1, ItemType.Water_L2 }
                },
                // Lv21-30：生成L1、L2、L3
                new LevelBracket
                {
                    minLevel = 21,
                    maxLevel = 30,
                    generateableItems = new[] { ItemType.Water_L1, ItemType.Water_L2, ItemType.Water_L3 }
                },
                // Lv31-40：生成L2、L3、L4
                new LevelBracket
                {
                    minLevel = 31,
                    maxLevel = 40,
                    generateableItems = new[] { ItemType.Water_L2, ItemType.Water_L3, ItemType.Water_L4 }
                },
                // Lv41-50：生成L3、L4、L5
                new LevelBracket
                {
                    minLevel = 41,
                    maxLevel = 50,
                    generateableItems = new[] { ItemType.Water_L3, ItemType.Water_L4, ItemType.Water_L5 }
                },
                // Lv51-60：生成L4、L5、L6
                new LevelBracket
                {
                    minLevel = 51,
                    maxLevel = 60,
                    generateableItems = new[] { ItemType.Water_L4, ItemType.Water_L5, ItemType.Water_L6 }
                },
                // Lv61+：生成L5、L6、L7
                new LevelBracket
                {
                    minLevel = 61,
                    maxLevel = 1000,
                    generateableItems = new[] { ItemType.Water_L5, ItemType.Water_L6, ItemType.Water_L7 }
                }
            };

            UnityEngine.Debug.Log("[ExplorationSystem] 已初始化7个等级段");
        }

        /// <summary>
        /// 初始化L1物品池（基础物品直接来源）
        /// 权重和：100%
        /// </summary>
        private void InitializeL1ItemPool()
        {
            _l1ItemPool = new L1ItemPoolEntry[]
            {
                new L1ItemPoolEntry { itemType = ItemType.Water_L1, probability = 0.20f },      // 净水 20%
                new L1ItemPoolEntry { itemType = ItemType.Food_L1, probability = 0.15f },       // 罐头 15%
                new L1ItemPoolEntry { itemType = ItemType.Tool_L1, probability = 0.15f },       // 零件 15%
                new L1ItemPoolEntry { itemType = ItemType.Home_L1, probability = 0.15f },       // 木材 15%
                new L1ItemPoolEntry { itemType = ItemType.Medical_L1, probability = 0.10f },    // 草药 10%
                new L1ItemPoolEntry { itemType = ItemType.Energy_L1, probability = 0.10f },     // 电池 10%
                new L1ItemPoolEntry { itemType = ItemType.Knowledge_L1, probability = 0.05f },  // 旧书 5%
                new L1ItemPoolEntry { itemType = ItemType.Hope_L1, probability = 0.03f },       // 种子 3%
                new L1ItemPoolEntry { itemType = ItemType.Explore_L1, probability = 0.05f },    // 地图碎片 5%
                // 布料比例较小，暂时跳过
            };

            UnityEngine.Debug.Log("[ExplorationSystem] 已初始化L1物品池，共9种物品");
        }

        /// <summary>
        /// 执行探索
        /// 返回获得的物品列表
        /// </summary>
        public ItemType[] Explore(int playerLevel)
        {
            // 1. 根据玩家等级确定产出等级范围
            var bracket = GetLevelBracket(playerLevel);
            
            // 2. 随机生成1-3个物品
            int itemCount = _random.Next(1, 4);  // 1-3个物品
            ItemType[] items = new ItemType[itemCount];

            for (int i = 0; i < itemCount; i++)
            {
                // 生成该等级段可生成的随机物品
                items[i] = GenerateRandomItem(bracket, playerLevel);
            }

            OnExploreSuccess?.Invoke(items);

            UnityEngine.Debug.Log($"[ExplorationSystem] 玩家Lv{playerLevel}进行探索，获得 {itemCount} 个物品");
            for (int i = 0; i < items.Length; i++)
            {
                UnityEngine.Debug.Log($"  [{i+1}] {ItemConfig.GetItemName(items[i])} L{ItemConfig.GetItemLevel(items[i])}");
            }

            return items;
        }

        /// <summary>
        /// 获取玩家对应的等级段
        /// </summary>
        private LevelBracket GetLevelBracket(int playerLevel)
        {
            foreach (var bracket in _levelBrackets)
            {
                if (playerLevel >= bracket.minLevel && playerLevel <= bracket.maxLevel)
                    return bracket;
            }

            // 默认返回最后一个（高等级）
            return _levelBrackets[_levelBrackets.Length - 1];
        }

        /// <summary>
        /// 根据等级段生成随机物品
        /// 实际实现中应该按概率表动态生成
        /// 这里简化为从该等级段可生成物品中随机选择
        /// </summary>
        private ItemType GenerateRandomItem(LevelBracket bracket, int playerLevel)
        {
            // 简化版：直接从L1物品池中选择
            // 实际应该按照设计规范4.2的概率表
            float rand = (float)_random.NextDouble();
            float cumulative = 0f;

            foreach (var entry in _l1ItemPool)
            {
                cumulative += entry.probability;
                if (rand <= cumulative)
                {
                    return entry.itemType;
                }
            }

            // 默认返回第一个
            return _l1ItemPool[0].itemType;
        }

        /// <summary>
        /// 获取探索消耗的体力
        /// </summary>
        public int GetExploreCost()
        {
            return 1;  // 固定消耗1点体力
        }

        /// <summary>
        /// 获取探索所得经验
        /// 设定：每次探索获得5×物品等级 的经验
        /// </summary>
        public int CalculateExploreExperience(ItemType[] items)
        {
            int exp = 0;
            foreach (var item in items)
            {
                exp += 5 * ItemConfig.GetItemLevel(item);
            }
            return exp;
        }
    }
}
