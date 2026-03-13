using System;
using TestWebGL.Game.Items;
using TestWebGL.Game.Grid;
using TestWebGL.Game.Player;

namespace TestWebGL.Game.Exploration
{
    /// <summary>
    /// 探索引擎 - 执行探索操作并处理物品放置
    /// 与CraftingEngine类似，负责游戏逻辑层面的探索交互
    /// </summary>
    public class ExplorationEngine
    {
        private ExplorationSystem _explorationSystem;
        private GridManager _gridManager;
        private PlayerManager _playerManager;

        // 事件
        public delegate void ExploreResultHandler(bool success, ItemType[] items, string message);
        public event ExploreResultHandler OnExploreResult;

        public delegate void PlacementFailedHandler(string reason);
        public event PlacementFailedHandler OnPlacementFailed;

        public ExplorationEngine(ExplorationSystem explorationSystem, GridManager gridManager, PlayerManager playerManager)
        {
            _explorationSystem = explorationSystem;
            _gridManager = gridManager;
            _playerManager = playerManager;
        }

        /// <summary>
        /// 尝试执行探索（点击探索按钮）
        /// 返回是否成功
        /// </summary>
        public bool TryExplore()
        {
            // 1. 检查体力是否充足
            int exploreCost = _explorationSystem.GetExploreCost();
            if (_playerManager.GetCurrentStamina() < exploreCost)
            {
                OnExploreResult?.Invoke(false, null, $"体力不足，需要{exploreCost}点体力");
                UnityEngine.Debug.Log($"[ExplorationEngine] 探索失败：体力不足");
                return false;
            }

            // 1. 检查网格是否已满
            if (IsGridFull())
            {
                OnExploreResult?.Invoke(false, null, "网格满了，无法探索");
                UnityEngine.Debug.Log($"[ExplorationEngine] 探索失败：网格满了");
                return false;
            }

            // 3. 进行探索获得物品
            ItemType[] items = _explorationSystem.Explore(_playerManager.GetLevel());

            // 4. 尝试将物品放在网格中（使用与拖动解锁相同的逻辑）
            if (TryPlaceExploredItems(items))
            {
                // 5. 消耗体力
                _playerManager.UseStamina(exploreCost);

                // 6. 获得经验
                int exp = _explorationSystem.CalculateExploreExperience(items);
                _playerManager.GainExperience(exp);

                OnExploreResult?.Invoke(true, items, $"成功探索，获得{items.Length}个物品，{exp}点经验");
                UnityEngine.Debug.Log($"[ExplorationEngine] 探索成功：消耗{exploreCost}体力，获得{items.Length}个物品，{exp}点经验");
                return true;
            }
            else
            {
                OnExploreResult?.Invoke(false, items, "物品无法全部放置，探索取消");
                UnityEngine.Debug.Log($"[ExplorationEngine] 探索失败：物品无法放置");
                return false;
            }
        }

        /// <summary>
        /// 尝试将探索得到的物品放在网格中
        /// 使用贪心算法，优先填充靠前靠左的空位
        /// </summary>
        private bool TryPlaceExploredItems(ItemType[] items)
        {
            // 1. 找到足够的空位数量
            int emptyCount = GetEmptyCellCount();
            if (emptyCount < items.Length)
            {
                UnityEngine.Debug.Log($"[ExplorationEngine] 空位数量不足：需要{items.Length}，只有{emptyCount}");
                return false;
            }

            // 2. 尝试放置每个物品
            foreach (var item in items)
            {
                bool placed = false;

                // 尝试在空位倒置，或堆叠
                for (int row = 0; row < 7; row++)  // 9x7 grid
                {
                    for (int col = 0; col < 9; col++)
                    {
                        var cell = _gridManager.GetCell(row, col);
                        if (cell == null) continue;

                        // 尝试放置新物品
                        if (!cell.HasItem && !cell.IsLocked)
                        {
                            var (placeSuccess, placeError) = _gridManager.TryPlaceItem(row, col, item, 1);
                            if (placeSuccess)
                            {
                                placed = true;
                                UnityEngine.Debug.Log($"[ExplorationEngine] 物品 {ItemConfig.GetItemName(item)} 放置在 ({row}, {col})");
                                break;
                            }
                            else
                            {
                                UnityEngine.Debug.Log($"[ExplorationEngine] 放置失败 ({row}, {col}): {placeError}");
                            }
                        }
                        // 如果该位置有相同物品，尝试堆叠
                        else if (cell.ItemCount > 0)
                        {
                            // 简化：仅放置在空位，不堆叠
                        }
                    }

                    if (placed) break;
                }

                if (!placed)
                {
                    UnityEngine.Debug.Log($"[ExplorationEngine] 物品 {ItemConfig.GetItemName(item)} 无法放置");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查网格是否已满（所有解锁格子都有物品）
        /// </summary>
        private bool IsGridFull()
        {
            var cells = _gridManager.GetAllCells();
            foreach (var cell in cells)
            {
                // 如果有解锁的空格子，说明网格未满
                if (!cell.IsLocked && !cell.HasItem)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取空格子的数量
        /// </summary>
        private int GetEmptyCellCount()
        {
            int count = 0;
            var cells = _gridManager.GetAllCells();
            foreach (var cell in cells)
            {
                if (!cell.IsLocked && !cell.HasItem)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 获取已填充格子的数量
        /// </summary>
        private int GetFilledCellCount()
        {
            int count = 0;
            var cells = _gridManager.GetAllCells();
            foreach (var cell in cells)
            {
                if (!cell.IsLocked && cell.HasItem)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
