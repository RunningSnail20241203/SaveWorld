using System;
using SaveWorld.Game.Core;
using SaveWorld.Game.Grid;

using SaveWorld.Game.Items;
using SaveWorld.Game.Grid;

namespace SaveWorld.Game.Crafting
{
    /// <summary>
    /// 合成引擎 - 处理游戏中的合成操作
    /// 包括双击合成、拖拽合成等
    /// </summary>
    public class CraftingEngine
    {
        private GridManager _gridManager;

        // 合成事件
        public delegate void CraftSuccessHandler(ItemType inputItem, int inputCount, ItemType outputItem, int outputCount);
        public event CraftSuccessHandler OnCraftSuccess;

        public delegate void CraftFailureHandler(string reason);
        public event CraftFailureHandler OnCraftFailure;

        public delegate void GridFullHandler();
        public event GridFullHandler OnGridFull;

        // 满格反馈事件
        public delegate void FullGridFeedbackHandler();
        public event FullGridFeedbackHandler OnFullGridFeedback;

        /// <summary>
        /// 初始化合成引擎
        /// </summary>
        public void Initialize(GridManager gridManager)
        {
            _gridManager = gridManager;

            UnityEngine.Debug.Log("[CraftingEngine] 合成引擎已初始化");
        }

        /// <summary>
        /// 双击合成 - 在指定格子处合成物品
        /// 流程：
        /// 1. 获取格子中的物品
        /// 2. 检查是否有2个或以上
        /// 3. 查找合成规则
        /// 4. 查找空格子放置输出物品
        /// 5. 消耗输入物品，生成输出物品
        /// </summary>
        public bool TryDoubleTapCraft(int row, int col)
        {
            var cell = _gridManager.GetCell(row, col);
            if (cell == null || _gridManager.IsCellLocked(row, col) || !cell.HasItem)
            {
                OnCraftFailure?.Invoke("格子无法合成");
                return false;
            }

            ItemType inputItem = cell.CurrentItemType;
            int inputCount = cell.ItemCount;

            // 检查是否可合成 (需要至少2个相同物品)
            if (inputCount < 2)
            {
                OnCraftFailure?.Invoke($"{ItemConfig.GetItemName(inputItem)} 数量不足（需要2个）");
                return false;
            }

            // 获取输出物品（下一级物品）
            ItemType outputItem = ItemConfig.GetNextLevelItem(inputItem);
            if (outputItem == ItemType.None)
            {
                OnCraftFailure?.Invoke("未找到合成规则");
                return false;
            }

            // 执行合成计算：2个合成1个
            ItemType outputType = outputItem;
            int outputCount = 1;

            // 计算合成后的输入物品剩余数量
            int remainingInput = inputCount % 2;

            // 尝试找到空格子放置输出物品
            int emptyRow, emptyCol;
            if (!FindEmptyCell(out emptyRow, out emptyCol))
            {
                // 没有空格，触发满格反馈
                OnFullGridFeedback?.Invoke();
                OnCraftFailure?.Invoke("没有空格子放置合成结果");
                return false;
            }

            // 执行操作：移除输入物品
            var (removeSuccess, removeError) = _gridManager.TryRemoveItem(row, col, inputCount - remainingInput);
            if (!removeSuccess)
            {
                UnityEngine.Debug.Log($"[CraftingEngine] 移除输入物品失败: {removeError}");
                OnCraftFailure?.Invoke($"移除物品失败 ({removeError})");
                return false;
            }

            // 如果有剩余物品，保留在原格子
            if (remainingInput == 0)
            {
                _gridManager.GetCell(row, col).ClearItem();
            }

            // 在空格子中放置输出物品
            var (placeSuccess, placeError) = _gridManager.TryPlaceItem(emptyRow, emptyCol, outputItem, outputCount);
            if (!placeSuccess)
            {
                UnityEngine.Debug.Log($"[CraftingEngine] 放置输出物品失败: {placeError}");
                OnCraftFailure?.Invoke($"放置物品失败 ({placeError})");
                return false;
            }

             // 记录已合成的物品（用于图鉴）
             GameLoop.Instance.EventBus.Dispatch(new ItemCraftedEvent(outputItem));

            // 触发成功事件
            OnCraftSuccess?.Invoke(inputItem, inputCount - remainingInput, outputItem, outputCount);

            UnityEngine.Debug.Log($"[CraftingEngine] 合成成功: {ItemConfig.GetItemName(inputItem)}×{inputCount - remainingInput} → " +
                                 $"{ItemConfig.GetItemName(outputItem)}×{outputCount} (放置在[{emptyRow},{emptyCol}])");

            return true;
        }

        /// <summary>
        /// 拖拽解锁 - 将物品拖拽到锁定格子解锁
        /// 规则：同类型、同等级的两个物品可以合并拖到锁格，解锁锁格
        /// </summary>
        public bool TryDragToUnlock(int fromRow, int fromCol, int toRow, int toCol)
        {
            // 检查来源格子
            var sourceCell = _gridManager.GetCell(fromRow, fromCol);
            if (sourceCell == null || _gridManager.IsCellLocked(fromRow, fromCol) || !sourceCell.HasItem || sourceCell.ItemCount < 1)
            {
                OnCraftFailure?.Invoke("来源格子无效");
                return false;
            }

            // 检查目标格子（必须是锁定的）
            var targetCell = _gridManager.GetCell(toRow, toCol);
            if (targetCell == null || !_gridManager.IsCellLocked(toRow, toCol))
            {
                OnCraftFailure?.Invoke("目标格子不是锁定格子");
                return false;
            }

            ItemType sourceItem = sourceCell.CurrentItemType;
            int sourceCount = sourceCell.ItemCount;
            ItemType lockedItem = _gridManager.GetLockedItemType(toRow, toCol);
            int lockedLevel = _gridManager.GetLockedItemLevel(toRow, toCol);

            // 检查是否匹配：同类型、同等级、数量足够（需要2个）
            if (sourceItem != lockedItem || ItemConfig.GetItemLevel(sourceItem) != lockedLevel || sourceCount < 2)
            {
                string reason = "";
                if (sourceItem != lockedItem)
                    reason = "物品类型不匹配";
                else if (ItemConfig.GetItemLevel(sourceItem) != lockedLevel)
                    reason = "物品等级不匹配";
                else
                    reason = "物品数量不足（需要2个）";

                OnCraftFailure?.Invoke(reason);
                return false;
            }

            // 执行解锁：消耗2个物品，解锁格子
            var (removeSuccess, removeError) = _gridManager.TryRemoveItem(fromRow, fromCol, 2);
            if (!removeSuccess)
            {
                UnityEngine.Debug.Log($"[CraftingEngine] 解锁时移除物品失败: {removeError}");
                OnCraftFailure?.Invoke($"移除物品失败 ({removeError})");
                return false;
            }
            
            _gridManager.TryUnlockCell(toRow, toCol);

             // 获得经验奖励（解锁格子奖励：20 × 物品等级）
             int expReward = 20 * lockedLevel;
             GameLoop.Instance.EventBus.Dispatch(new ExperienceGainedEvent(expReward, $"解锁格子（{ItemConfig.GetItemName(lockedItem)}）"));

            OnCraftSuccess?.Invoke(sourceItem, 2, ItemType.None, 0);

            UnityEngine.Debug.Log($"[CraftingEngine] 解锁成功: {ItemConfig.GetItemName(lockedItem)}L{lockedLevel} 格子已解锁！ " +
                                 $"获得 {expReward} 经验");

            return true;
        }

        /// <summary>
        /// 查找第一个空格子
        /// 优先级：从左到右、从上到下
        /// </summary>
        private bool FindEmptyCell(out int emptyRow, out int emptyCol)
        {
            foreach (var cell in _gridManager.GetAllCells())
            {
                if (!_gridManager.IsCellLocked(cell.row, cell.column) && !cell.HasItem)
                {
                    emptyRow = cell.row;
                    emptyCol = cell.column;
                    return true;
                }
            }

            emptyRow = -1;
            emptyCol = -1;
            return false;
        }

        /// <summary>
        /// 检查格子是否满
        /// 满 = 所有未锁定格子都有物品（没有空格）
        /// </summary>
        public bool IsGridFull()
        {
            foreach (var cell in _gridManager.GetAllCells())
            {
                if (!_gridManager.IsCellLocked(cell.row, cell.column) && !cell.HasItem)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 获取空格子数量
        /// </summary>
        public int GetEmptyCellCount()
        {
            int count = 0;
            foreach (var cell in _gridManager.GetAllCells())
            {
                if (!_gridManager.IsCellLocked(cell.row, cell.column) && !cell.HasItem)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 获取已填充格子数量
        /// </summary>
        public int GetFilledCellCount()
        {
            int count = 0;
            foreach (var cell in _gridManager.GetAllCells())
            {
                if (!_gridManager.IsCellLocked(cell.row, cell.column) && cell.HasItem)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 拖拽合并 - 将一个格子的物品拖拽到另一个格子进行合并
        /// 规则：
        /// 1. 相同类型物品可以堆叠
        /// 2. 堆叠后如果数量≥2，自动触发合成
        /// 3. 支持跨格子合成
        /// </summary>
        public bool TryDragMerge(int fromRow, int fromCol, int toRow, int toCol)
        {
            // 相同格子直接返回
            if (fromRow == toRow && fromCol == toCol)
                return false;

            // 检查来源格子
            var sourceCell = _gridManager.GetCell(fromRow, fromCol);
            if (sourceCell == null || _gridManager.IsCellLocked(fromRow, fromCol) || !sourceCell.HasItem)
            {
                OnCraftFailure?.Invoke("来源格子无效");
                return false;
            }

            // 检查目标格子
            var targetCell = _gridManager.GetCell(toRow, toCol);
            if (targetCell == null || _gridManager.IsCellLocked(toRow, toCol))
            {
                OnCraftFailure?.Invoke("目标格子无效");
                return false;
            }

            ItemType sourceItem = sourceCell.CurrentItemType;
            int sourceCount = sourceCell.ItemCount;

            // 目标格子为空：直接移动
            if (!targetCell.HasItem)
            {
                var (moveSuccess, moveError) = _gridManager.TryMoveItem(fromRow, fromCol, toRow, toCol);
                if (!moveSuccess)
                {
                    OnCraftFailure?.Invoke($"移动物品失败 ({moveError})");
                    return false;
                }
                
                UnityEngine.Debug.Log($"[CraftingEngine] 物品移动成功: {ItemConfig.GetItemName(sourceItem)} ×{sourceCount} 从 [{fromRow},{fromCol}] → [{toRow},{toCol}]");
                return true;
            }

            // 目标格子有物品：检查是否同类型
            ItemType targetItem = targetCell.CurrentItemType;
            if (sourceItem != targetItem)
            {
                OnCraftFailure?.Invoke("不同类型物品无法合并");
                return false;
            }

            int totalCount = sourceCount + targetCell.ItemCount;
            
            // 移除两个格子的所有物品
            _gridManager.TryRemoveItem(fromRow, fromCol, sourceCount);
            _gridManager.TryRemoveItem(toRow, toCol, targetCell.ItemCount);

            // 计算可以合成多少次
            int craftCount = totalCount / 2;
            int remainingCount = totalCount % 2;

            ItemType outputItem = ItemConfig.GetNextLevelItem(sourceItem);
            
            // 有可以合成的
            if (craftCount > 0 && outputItem != ItemType.None)
            {
                // 放置合成产物
                if (FindEmptyCell(out int emptyRow, out int emptyCol))
                {
                    _gridManager.TryPlaceItem(emptyRow, emptyCol, outputItem, craftCount);
                    
                    // 派发合成事件
                    GameLoop.Instance.EventBus.Dispatch(new ItemCraftedEvent(outputItem));
                    OnCraftSuccess?.Invoke(sourceItem, craftCount * 2, outputItem, craftCount);
                    
                    UnityEngine.Debug.Log($"[CraftingEngine] 拖拽合成成功: {ItemConfig.GetItemName(sourceItem)} ×{craftCount * 2} → {ItemConfig.GetItemName(outputItem)} ×{craftCount}");
                }
                else
                {
                    // 没有空格子，还原物品
                    _gridManager.TryPlaceItem(fromRow, fromCol, sourceItem, sourceCount);
                    _gridManager.TryPlaceItem(toRow, toCol, targetItem, targetCell.ItemCount);
                    
                    OnFullGridFeedback?.Invoke();
                    OnCraftFailure?.Invoke("没有空格子放置合成结果");
                    return false;
                }
            }

            // 放置剩余物品
            if (remainingCount > 0)
            {
                _gridManager.TryPlaceItem(toRow, toCol, sourceItem, remainingCount);
            }
            else
            {
                targetCell.ClearItem();
            }

            sourceCell.ClearItem();

            return true;
        }

        /// <summary>
        /// 一键全合成 - 自动扫描背包中所有可合成物品，全部合成到不能合成为止
        /// 返回合成总次数
        /// </summary>
        public int TryAutoCraftAll()
        {
            int totalCrafted = 0;
            bool hasCrafted;

            do
            {
                hasCrafted = false;
                
                // 从低等级到高等级遍历所有格子
                foreach (var cell in _gridManager.GetAllCells().OrderBy(c => ItemConfig.GetItemLevel(c.CurrentItemType)))
                {
                    if (_gridManager.IsCellLocked(cell.row, cell.column) || !cell.HasItem)
                        continue;
                    
                    if (cell.ItemCount >= 2 && ItemConfig.GetNextLevelItem(cell.CurrentItemType) != ItemType.None)
                    {
                        if (TryDoubleTapCraft(cell.row, cell.column))
                        {
                            totalCrafted++;
                            hasCrafted = true;
                            break; // 合成后重新扫描，因为物品位置变化了
                        }
                    }
                }

            } while (hasCrafted);

            if (totalCrafted > 0)
            {
                UnityEngine.Debug.Log($"[CraftingEngine] 一键全合成完成，共合成 {totalCrafted} 次");
            }
            else
            {
                OnCraftFailure?.Invoke("没有可以合成的物品");
            }

            return totalCrafted;
        }
    }
}
