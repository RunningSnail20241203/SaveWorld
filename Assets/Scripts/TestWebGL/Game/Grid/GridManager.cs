using System.Collections.Generic;
using TestWebGL.Game.Items;

namespace TestWebGL.Game.Grid
{
    /// <summary>
    /// 格子系统管理器
    /// 管理9×7 = 63个格子，包括锁定、解锁、物品管理等
    /// 
    /// 【重构 v2.0】
    /// 锁定机制与格子数据完全分离
    /// GridCell 只负责持有物品，GridManager 统一管理锁定状态
    /// 锁定是正交的外部机制，可以动态添加任何类型的锁定
    /// </summary>
    public class GridManager
    {
        // 格子配置
        private const int GRID_ROWS = 9;
        private const int GRID_COLS = 7;
        private const int TOTAL_CELLS = GRID_ROWS * GRID_COLS;  // 63

        private const int UNLOCKED_CENTER_SIZE = 3;  // 初始解锁3×3 = 9格
        private const int UNLOCKED_CENTER_START = 3; // 中心开始行列（从0计数，则中心是3,4,5）

        // 格子二维数组 - 只存物品数据
        private GridCell[,] _gridCells;
        
        // 🔒 外部锁定机制 - 与格子数据完全分离
        private bool[,] _isLocked;                  // 锁定状态
        private ItemType[,] _lockedItemType;        // 解锁需要的物品类型
        private int[,] _lockedItemLevel;            // 解锁需要的物品等级

        // 格子状态变化事件
        public delegate void GridCellChangedHandler(int row, int col, GridCell cell);
        public event GridCellChangedHandler OnCellChanged;

        public delegate void GridUnlockedHandler(int row, int col, GridCell cell);
        public event GridUnlockedHandler OnGridUnlocked;

        /// <summary>
        /// 初始化格子系统
        /// </summary>
        public void Initialize()
        {
            _gridCells = new GridCell[GRID_ROWS, GRID_COLS];
            _isLocked = new bool[GRID_ROWS, GRID_COLS];
            _lockedItemType = new ItemType[GRID_ROWS, GRID_COLS];
            _lockedItemLevel = new int[GRID_ROWS, GRID_COLS];

            // 1. 创建所有空格子
            InitializeAllCells();

            // 2. 设置初始解锁的中心3×3区域
            UnlockCenterArea();

            // 3. 应用初始锁定机制
            ApplyInitialLockingMechanism();
        }

        /// <summary>
        /// 初始化所有格子为空格子
        /// </summary>
        private void InitializeAllCells()
        {
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    _gridCells[row, col] = GridCell.CreateEmpty(row, col);
                    _isLocked[row, col] = true;
                    _lockedItemType[row, col] = ItemType.None;
                    _lockedItemLevel[row, col] = 0;
                }
            }
        }

        /// <summary>
        /// 解锁中心3×3区域
        /// 中心位置：行3-5，列3-5（基于9×7网格的中心）
        /// </summary>
        private void UnlockCenterArea()
        {
            for (int row = UNLOCKED_CENTER_START; row < UNLOCKED_CENTER_START + UNLOCKED_CENTER_SIZE; row++)
            {
                for (int col = UNLOCKED_CENTER_START; col < UNLOCKED_CENTER_START + UNLOCKED_CENTER_SIZE; col++)
                {
                    _isLocked[row, col] = false;
                }
            }
        }

        /// <summary>
        /// 应用初始锁定机制
        /// 根据设计规范中的3.2节分配锁格
        /// 这只是其中一种锁定机制，以后可以添加更多
        /// </summary>
        private void ApplyInitialLockingMechanism()
        {
            var lockedGridConfig = new[,]
            {
                // 行1（索引0）
                { ItemType.Water_L3, ItemType.Home_L2, ItemType.Food_L3, ItemType.Energy_L1, ItemType.Knowledge_L2, ItemType.Medical_L2, ItemType.Energy_L5 },
                // 行2（索引1）
                { ItemType.Tool_L3, ItemType.Water_L2, ItemType.Hope_L2, ItemType.Knowledge_L1, ItemType.Food_L2, ItemType.Tool_L2, ItemType.Medical_L4 },
                // 行3（索引2）
                { ItemType.Home_L4, ItemType.Energy_L2, ItemType.None, ItemType.None, ItemType.None, ItemType.Medical_L3, ItemType.Energy_L4 },
                // 行4（索引3）
                { ItemType.Medical_L1, ItemType.Explore_L2, ItemType.None, ItemType.None, ItemType.None, ItemType.Explore_L3, ItemType.Home_L1 },
                // 行5（索引4）
                { ItemType.Tool_L1, ItemType.Food_L1, ItemType.None, ItemType.None, ItemType.None, ItemType.Home_L2, ItemType.Water_L1 },
                // 行6（索引5）
                { ItemType.Water_L2, ItemType.Knowledge_L2, ItemType.Medical_L2, ItemType.Knowledge_L1, ItemType.Food_L2, ItemType.Energy_L2, ItemType.Water_L3 },
                // 行7（索引6）
                { ItemType.Hope_L3, ItemType.Medical_L3, ItemType.Medical_L4, ItemType.Explore_L3, ItemType.Energy_L4, ItemType.Energy_L5, ItemType.Medical_L5 },
                // 行8（索引7）
                { ItemType.Energy_L8, ItemType.Water_L5, ItemType.Tool_L5, ItemType.Medical_L5, ItemType.Knowledge_L5, ItemType.Hope_L5, ItemType.Explore_L7 },
                // 行9（索引8）
                { ItemType.Home_L9, ItemType.Energy_L9, ItemType.Medical_L9, ItemType.Knowledge_L10, ItemType.Water_L10, ItemType.Home_L10, ItemType.Explore_L10 },
            };

            // 应用锁格配置
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    ItemType lockedItemType = lockedGridConfig[row, col];

                    // 如果是None或中心区域，保持解锁状态
                    if (lockedItemType == ItemType.None || IsInCenterArea(row, col))
                        continue;

                    // 应用锁定
                    _isLocked[row, col] = true;
                    _lockedItemType[row, col] = lockedItemType;
                    _lockedItemLevel[row, col] = ItemConfig.GetItemLevel(lockedItemType);
                }
            }
        }

        /// <summary>
        /// 检查是否在中心解锁区域
        /// </summary>
        private bool IsInCenterArea(int row, int col)
        {
            return row >= UNLOCKED_CENTER_START && row < UNLOCKED_CENTER_START + UNLOCKED_CENTER_SIZE &&
                   col >= UNLOCKED_CENTER_START && col < UNLOCKED_CENTER_START + UNLOCKED_CENTER_SIZE;
        }

        /// <summary>
        /// 查询格子是否被锁定
        /// 🔒 这是唯一的锁定查询入口
        /// </summary>
        public bool IsCellLocked(int row, int col)
        {
            if (!IsValidPosition(row, col))
                return false;
            
            return _isLocked[row, col];
        }

        /// <summary>
        /// 获取锁定格子需要的解锁物品
        /// </summary>
        public ItemType GetLockedItemType(int row, int col)
        {
            if (!IsValidPosition(row, col))
                return ItemType.None;
            
            return _lockedItemType[row, col];
        }

        /// <summary>
        /// 获取锁定格子需要的解锁物品等级
        /// </summary>
        public int GetLockedItemLevel(int row, int col)
        {
            if (!IsValidPosition(row, col))
                return 0;
            
            return _lockedItemLevel[row, col];
        }

        /// <summary>
        /// 锁定一个格子
        /// </summary>
        public void LockCell(int row, int col, ItemType requiredItem = ItemType.None, int requiredLevel = 0)
        {
            if (!IsValidPosition(row, col))
                return;

            _isLocked[row, col] = true;
            _lockedItemType[row, col] = requiredItem;
            _lockedItemLevel[row, col] = requiredLevel;
            
            OnCellChanged?.Invoke(row, col, _gridCells[row, col]);
        }

        /// <summary>
        /// 解锁一个格子
        /// </summary>
        public void UnlockCell(int row, int col)
        {
            if (!IsValidPosition(row, col))
                return;

            _isLocked[row, col] = false;
            _lockedItemType[row, col] = ItemType.None;
            _lockedItemLevel[row, col] = 0;
            
            OnGridUnlocked?.Invoke(row, col, _gridCells[row, col]);
            OnCellChanged?.Invoke(row, col, _gridCells[row, col]);
        }

        /// <summary>
        /// 获取某个格子
        /// </summary>
        public GridCell GetCell(int row, int col)
        {
            if (IsValidPosition(row, col))
                return _gridCells[row, col];
            return null;
        }

        /// <summary>
        /// 验证位置是否有效
        /// </summary>
        public bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < GRID_ROWS && col >= 0 && col < GRID_COLS;
        }

        /// <summary>
        /// 获取所有格子（用于UI渲染）
        /// </summary>
        public IEnumerable<GridCell> GetAllCells()
        {
            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    yield return _gridCells[row, col];
                }
            }
        }

        /// <summary>
        /// 在格子中放置物品
        /// 如果格子已有同类型物品，自动堆叠
        /// 返回(success, errorCode)
        /// </summary>
        public (bool success, PlacementErrorCode errorCode) TryPlaceItem(int row, int col, ItemType itemType, int count = 1)
        {
            if (!IsValidPosition(row, col))
                return (false, PlacementErrorCode.InvalidPosition);

            // 🔒 锁定检查 - 统一在这里做
            if (IsCellLocked(row, col))
                return (false, PlacementErrorCode.CellLocked);

            GridCell cell = _gridCells[row, col];

            // 尝试堆叠
            if (cell.CurrentItemType == itemType)
            {
                bool stacked = cell.TryStackItem(itemType, count);
                if (stacked)
                {
                    OnCellChanged?.Invoke(row, col, cell);
                    return (true, PlacementErrorCode.None);
                }
                else
                {
                    return (false, PlacementErrorCode.StackLimitExceeded);
                }
            }

            // 检查格子是否已有其他物品
            if (cell.HasItem)
                return (false, PlacementErrorCode.CellOccupied);

            // 尝试放置到空格子
            bool placed = cell.TryPlaceItem(itemType, count);
            if (placed)
            {
                OnCellChanged?.Invoke(row, col, cell);
                return (true, PlacementErrorCode.None);
            }
            else
            {
                return (false, PlacementErrorCode.PlacementFailed);
            }
        }

        /// <summary>
        /// 从格子移除物品
        /// 返回(success, errorCode)
        /// </summary>
        public (bool success, RemovalErrorCode errorCode) TryRemoveItem(int row, int col, int removeCount)
        {
            if (!IsValidPosition(row, col))
                return (false, RemovalErrorCode.InvalidPosition);

            // 🔒 锁定检查 - 统一在这里做
            if (IsCellLocked(row, col))
                return (false, RemovalErrorCode.CellLocked);

            GridCell cell = _gridCells[row, col];
            
            if (!cell.HasItem)
                return (false, RemovalErrorCode.CellEmpty);

            if (cell.ItemCount < removeCount)
                return (false, RemovalErrorCode.InsufficientItems);

            bool removed = cell.TryRemoveItem(removeCount);
            if (removed)
            {
                OnCellChanged?.Invoke(row, col, cell);
                return (true, RemovalErrorCode.None);
            }
            else
            {
                return (false, RemovalErrorCode.RemovalFailed);
            }
        }

        /// <summary>
        /// 设置格子状态（用于内部更新）
        /// </summary>
        public void SetCell(int row, int col, GridCell newCell)
        {
            if (!IsValidPosition(row, col))
                return;

            _gridCells[row, col] = newCell;
            OnCellChanged?.Invoke(row, col, newCell);
        }

        /// <summary>
        /// 尝试解锁格子
        /// </summary>
        public bool TryUnlockCell(int row, int col)
        {
            if (!IsValidPosition(row, col))
                return false;

            if (!IsCellLocked(row, col))
                return false;

            UnlockCell(row, col);
            return true;
        }

        /// <summary>
        /// 获取格子统计信息
        /// </summary>
        public GridStatistics GetStatistics()
        {
            var stats = new GridStatistics();

            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    GridCell cell = _gridCells[row, col];
                    if (IsCellLocked(row, col))
                        stats.lockedCellCount++;
                    else if (cell.HasItem)
                        stats.filledCellCount++;
                    else
                        stats.emptyCellCount++;

                    stats.totalItemCount += cell.ItemCount;
                }
            }

            stats.totalCellCount = TOTAL_CELLS;
            return stats;
        }

        /// <summary>
        /// 获取格子尺寸信息
        /// </summary>
        public GridDimensions GetDimensions()
        {
            return new GridDimensions
            {
                rows = GRID_ROWS,
                cols = GRID_COLS,
                totalCells = TOTAL_CELLS,
                cellSize = 120,  // 像素 - 适配1080*1920分辨率
                cellSpacing = 8   // 像素 - 适配1080*1920分辨率
            };
        }
    }

    /// <summary>
    /// 格子统计数据
    /// </summary>
    [System.Serializable]
    public class GridStatistics
    {
        public int totalCellCount;
        public int lockedCellCount;
        public int filledCellCount;
        public int emptyCellCount;
        public int totalItemCount;
    }

    /// <summary>
    /// 格子尺寸信息
    /// </summary>
    [System.Serializable]
    public class GridDimensions
    {
        public int rows;
        public int cols;
        public int totalCells;
        public int cellSize;      // 单个格子像素大小
        public int cellSpacing;   // 格子间距
    }

    /// <summary>
    /// 放置操作的错误码
    /// </summary>
    public enum PlacementErrorCode
    {
        None = 0,
        InvalidPosition = 1,    // 位置无效
        CellLocked = 2,         // 格子被锁定
        StackLimitExceeded = 3, // 堆叠数量超限
        CellOccupied = 4,       // 格子已有其他物品
        PlacementFailed = 5,    // 放置操作异常
    }

    /// <summary>
    /// 移除操作的错误码
    /// </summary>
    public enum RemovalErrorCode
    {
        None = 0,
        InvalidPosition = 1,    // 位置无效
        CellLocked = 2,         // 格子被锁定
        CellEmpty = 3,          // 格子无物品
        InsufficientItems = 4,  // 物品数量不足
        RemovalFailed = 5,      // 移除操作异常
    }
}