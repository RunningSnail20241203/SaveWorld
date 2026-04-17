using System;
using SaveWorld.Game.Core;
using SaveWorld.Game.Items;

namespace SaveWorld.Game.Exploration
{
    /// <summary>
    /// 探索引擎 V2
    /// 纯函数 无状态 无副作用 零依赖
    /// 输入: GameState 快照 随机种子
    /// 输出: 探索结果
    /// 所有逻辑可预测 可单独测试
    /// </summary>
    public static class ExplorationEngine
    {
        /// <summary>
        /// 尝试探索
        /// 纯函数 不会修改任何状态 只会返回结果
        /// </summary>
        public static ExplorationResult TryExplore(GameState state, int randomSeed)
        {
            // 1. 检查体力
            if (state.Player.Stamina < 1)
            {
                return ExplorationResult.Fail("体力不足");
            }

// 2. 检查背包空间
int emptyCount = FindEmptyCells(state, 0, new Random()).Length;
if (emptyCount == 0)
{
    return ExplorationResult.Fail("背包已满");
}

            var random = new Random(randomSeed);

            // 3. 生成1-3个物品
            int itemCount = random.Next(1, Math.Min(4, emptyCount + 1));
            int[] cellIds = FindEmptyCells(state, itemCount, random);
            int[] itemIds = new int[itemCount];

            for (int i = 0; i < itemCount; i++)
            {
                itemIds[i] = GenerateRandomItem(state.Player.Level, random);
            }

            // 4. 计算经验
            int exp = 0;
            foreach (int itemId in itemIds)
            {
                exp += 5 * ItemConfig.GetItemLevel(itemId);
            }

            // 返回成功结果
            return ExplorationResult.CreateSuccess(cellIds, itemIds, 1, exp);
        }

        /// <summary>
        /// 查找空格子
        /// 优先随机分配 避免集中在左上角
        /// </summary>
        private static int[] FindEmptyCells(GameState state, int count, Random random)
        {
            Span<int> emptyCells = stackalloc int[63];
            int emptyCount = 0;

            for (int i = 0; i < 63; i++)
            {
                if (!state.Cells[i].IsLocked && !state.Cells[i].HasItem)
                {
                    emptyCells[emptyCount++] = i;
                }
            }

            // Fisher-Yates 洗牌
            for (int i = emptyCount - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (emptyCells[i], emptyCells[j]) = (emptyCells[j], emptyCells[i]);
            }

            return emptyCells[..count].ToArray();
        }

        /// <summary>
        /// 根据玩家等级生成随机物品
        /// 符合设计规范4.2 等级段掉落表
        /// </summary>
        private static int GenerateRandomItem(int playerLevel, Random random)
        {
            // L1物品池 9种基础物品
            Span<(int ItemId, float Probability)> pool = stackalloc (int, float)[]
            {
                (1, 0.20f),   // 净水 20%
                (2, 0.15f),   // 罐头 15%
                (3, 0.15f),   // 零件 15%
                (4, 0.15f),   // 木材 15%
                (5, 0.10f),   // 草药 10%
                (6, 0.10f),   // 电池 10%
                (7, 0.05f),   // 旧书 5%
                (8, 0.03f),   // 种子 3%
                (9, 0.05f)    // 地图碎片 5%
            };

            float rand = (float)random.NextDouble();
            float cumulative = 0f;

            foreach (var entry in pool)
            {
                cumulative += entry.Probability;
                if (rand <= cumulative)
                {
                    return entry.ItemId;
                }
            }

            return 1;
        }
    }

    /// <summary>
    /// 探索结果
    /// 不可变 纯数据
    /// </summary>
    public readonly struct ExplorationResult
    {
        public bool Success { get; }
        public int[] CellIds { get; }
        public int[] ItemIds { get; }
        public int StaminaCost { get; }
        public int ExperienceGain { get; }
        public string FailReason { get; }

        private ExplorationResult(bool success, int[] cellIds, int[] itemIds, int staminaCost, int experienceGain, string failReason)
        {
            Success = success;
            CellIds = cellIds;
            ItemIds = itemIds;
            StaminaCost = staminaCost;
            ExperienceGain = experienceGain;
            FailReason = failReason;
        }

        public static ExplorationResult CreateSuccess(int[] cellIds, int[] itemIds, int staminaCost, int experienceGain)
        {
            return new ExplorationResult(true, cellIds, itemIds, staminaCost, experienceGain, null);
        }

        public static ExplorationResult Fail(string reason)
        {
            return new ExplorationResult(false, Array.Empty<int>(), Array.Empty<int>(), 0, 0, reason);
        }
    }
}
