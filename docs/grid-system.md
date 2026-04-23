# 格子系统 (Grid System)

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/Grid/`

---

## 概述

管理 9×7 = 63 个格子的背包系统。中心 3×3 = 9 格初始解锁，其余 54 格需要消耗特定物品解锁。

---

## GridManager

**文件**: `Grid/GridManager.cs` | **命名空间**: `SaveWorld.Game.Grid`

### 职责
- 管理 63 个格子的创建、查询、修改
- 统一管理锁定状态（与格子数据分离的正交机制）
- 处理物品放置、移除、堆叠

### 常量

| 常量 | 值 | 说明 |
|------|-----|------|
| `GRID_ROWS` | 9 | 行数 |
| `GRID_COLS` | 7 | 列数 |
| `TOTAL_CELLS` | 63 | 总格子数 |
| `UNLOCKED_CENTER_SIZE` | 3 | 中心解锁区域大小 |
| `UNLOCKED_CENTER_START` | 3 | 中心区域起始行列 |

### 事件

```csharp
public delegate void GridCellChangedHandler(int row, int col, GridCell cell);
public event GridCellChangedHandler OnCellChanged;

public delegate void GridUnlockedHandler(int row, int col, GridCell cell);
public event GridUnlockedHandler OnGridUnlocked;
```

### 核心方法

```csharp
// 初始化
public void Initialize()

// 锁定查询
public bool IsCellLocked(int row, int col)
public ItemType GetLockedItemType(int row, int col)
public int GetLockedItemLevel(int row, int col)

// 格子操作
public GridCell GetCell(int row, int col)
public (bool success, PlacementErrorCode errorCode) TryPlaceItem(int row, int col, ItemType itemType, int count = 1)
public (bool success, RemovalErrorCode errorCode) TryRemoveItem(int row, int col, int removeCount)
public void SetCell(int row, int col, GridCell newCell)
public bool TryUnlockCell(int row, int col)

// 查询
public IEnumerable<GridCell> GetAllCells()
public GridStatistics GetStatistics()
public GridDimensions GetDimensions()
```

### 锁格配置

9×7 网格的锁格分布（除中心 3×3 外）：

```
行1: [净水L3][住所L2][食物L3][能源L1][知识L2][医疗L2][能源L5]
行2: [工具L3][水源L2][希望L2][知识L1][食物L2][工具L2][医疗L4]
行3: [住所L4][能源L2][ 空  ][ 空  ][ 空  ][医疗L3][能源L4]
行4: [医疗L1][探索L2][ 空  ][ 空  ][ 空  ][探索L3][住所L1]
行5: [工具L1][食物L1][ 空  ][ 空  ][ 空  ][住所L2][水源L1]
行6: [水源L2][知识L2][医疗L2][知识L1][食物L2][能源L2][水源L3]
行7: [希望L3][医疗L3][医疗L4][探索L3][能源L4][能源L5][医疗L5]
行8: [能源L8][水源L5][工具L5][医疗L5][知识L5][希望L5][探索L7]
行9: [住所L9][能源L9][医疗L9][知识L10][水源L10][住所L10][探索L10]
```

### 错误码

**PlacementErrorCode**
- `None = 0` - 成功
- `InvalidPosition = 1` - 位置无效
- `CellLocked = 2` - 格子被锁定
- `StackLimitExceeded = 3` - 堆叠超限(>99)
- `CellOccupied = 4` - 格子已有其他物品
- `PlacementFailed = 5` - 放置异常

**RemovalErrorCode**
- `None = 0` - 成功
- `InvalidPosition = 1` - 位置无效
- `CellLocked = 2` - 格子被锁定
- `CellEmpty = 3` - 格子无物品
- `InsufficientItems = 4` - 物品数量不足
- `RemovalFailed = 5` - 移除异常

---

## GridCell

**文件**: `Grid/GridCell.cs` | **类型**: `[Serializable] class`

### 职责
只负责持有物品数据，不包含锁定状态（锁定由 GridManager 统一管理）。

### 字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `row` | `int` | 行索引 |
| `column` | `int` | 列索引 |
| `_currentItemType` | `ItemType` | 当前物品类型 |
| `_itemCount` | `int` | 堆叠数量 (1-99) |

### 属性

```csharp
public ItemType CurrentItemType => _currentItemType;
public int ItemCount => _itemCount;
public bool HasItem => _currentItemType != ItemType.None && _itemCount > 0;
```

### 核心方法

```csharp
public static GridCell CreateEmpty(int row, int column)
public static GridCell CreateFilled(int row, int column, ItemType itemType, int count = 1)
public bool TryPlaceItem(ItemType itemType, int count = 1)      // 放置到空格子
public bool TryStackItem(ItemType itemType, int addCount = 1)   // 堆叠同类物品
public bool TryRemoveItem(int removeCount)                      // 移除物品
public void ClearItem()                                         // 清空格子
public string GetDisplayName()                                  // 获取显示名称
```

### 数据模型

```
┌─────────────────────────────────────┐
│           GridCell                  │
├─────────────────────────────────────┤
│  row: int                           │
│  column: int                        │
│  _currentItemType: ItemType         │
│  _itemCount: int (1-99)             │
├─────────────────────────────────────┤
│  + HasItem: bool                    │
│  + CurrentItemType: ItemType        │
│  + ItemCount: int                   │
└─────────────────────────────────────┘
```

---

## 辅助数据结构

### GridStatistics

```csharp
public class GridStatistics
{
    public int totalCellCount;    // 总格子数
    public int lockedCellCount;   // 锁定格子数
    public int filledCellCount;   // 已填充格子数
    public int emptyCellCount;    // 空格子数
    public int totalItemCount;    // 总物品数
}
```

### GridDimensions

```csharp
public class GridDimensions
{
    public int rows;           // 9
    public int cols;           // 7
    public int totalCells;     // 63
    public int cellSize;       // 120 像素
    public int cellSpacing;    // 8 像素
}
```

---

## 设计要点

1. **锁定与数据分离**: `GridCell` 只持有物品，`GridManager` 统一管锁定
2. **正交锁定机制**: 锁格可以动态添加不同类型（物品解锁、等级解锁等）
3. **堆叠上限**: 99 个/格
4. **中心 3×3 初始解锁**: 行 3-5，列 3-5（0 基索引）
5. **54 锁格**: 每个锁格需要特定类型和等级的物品 ×2 解锁
