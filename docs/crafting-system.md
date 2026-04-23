# 合成引擎 (Crafting Engine)

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/Crafting/CraftingEngine.cs`

---

## 概述

处理游戏中的合成操作，包括双击合成、拖拽合成、拖拽解锁、一键全合成。

核心规则：**2 个相同类型、相同等级的物品 → 合成 1 个下一级物品**。

---

## CraftingEngine

**文件**: `Crafting/CraftingEngine.cs` | **命名空间**: `SaveWorld.Game.Crafting`

### 职责
- 双击合成（同格子内 2 合 1）
- 拖拽合并（跨格子 2 合 1）
- 拖拽解锁（消耗 2 个物品解锁锁格）
- 一键全合成（自动扫描所有可合成物品）
- 满格检测与反馈

### 事件

```csharp
public delegate void CraftSuccessHandler(ItemType inputItem, int inputCount, ItemType outputItem, int outputCount);
public event CraftSuccessHandler OnCraftSuccess;

public delegate void CraftFailureHandler(string reason);
public event CraftFailureHandler OnCraftFailure;

public delegate void GridFullHandler();
public event GridFullHandler OnGridFull;

public delegate void FullGridFeedbackHandler();
public event FullGridFeedbackHandler OnFullGridFeedback;
```

### 核心方法

```csharp
// 初始化
public void Initialize(GridManager gridManager)

// 双击合成 - 在指定格子处合成
public bool TryDoubleTapCraft(int row, int col)

// 拖拽解锁 - 将物品拖拽到锁定格子解锁
public bool TryDragToUnlock(int fromRow, int fromCol, int toRow, int toCol)

// 拖拽合并 - 将一个格子物品拖拽到另一个格子合并
public bool TryDragMerge(int fromRow, int fromCol, int toRow, int toCol)

// 一键全合成 - 自动扫描背包中所有可合成物品
public int TryAutoCraftAll()

// 检查格子是否满
public bool IsGridFull()

// 获取空格子数量
public int GetEmptyCellCount()

// 获取已填充格子数量
public int GetFilledCellCount()
```

---

## 合成流程

### 双击合成 (TryDoubleTapCraft)

```
1. 获取格子物品
2. 检查是否有 2 个或以上
3. 查找合成规则 (ItemConfig.GetNextLevelItem)
4. 查找空格子放置输出物品
5. 消耗输入物品，生成输出物品
6. 发布 ItemCraftedEvent + ExperienceGainedEvent
```

**经验计算**: `产出等级 × 数量`

### 拖拽解锁 (TryDragToUnlock)

```
1. 检查来源格子有物品 (≥1)
2. 检查目标格子是锁定的
3. 检查物品类型匹配 + 等级匹配 + 数量 ≥ 2
4. 消耗 2 个物品
5. 解锁格子
6. 发布 ExperienceGainedEvent (20 × 物品等级)
```

### 拖拽合并 (TryDragMerge)

```
1. 检查来源和目标格子有效
2. 目标为空 → 直接移动
3. 目标有物品 → 检查是否同类型
4. 合并计算: (sourceCount + targetCount) / 2 = craftCount
5. 剩余: (sourceCount + targetCount) % 2 = remainingCount
6. 放置合成产物到空格子
7. 放置剩余物品到目标格子
```

### 一键全合成 (TryAutoCraftAll)

```
do {
    hasCrafted = false;
    // 从低等级到高等级遍历
    foreach (cell in GetAllCells().OrderBy(level)) {
        if (cell.ItemCount >= 2 && HasNextLevel) {
            TryDoubleTapCraft(cell.row, cell.column);
            hasCrafted = true;
            break; // 重新扫描
        }
    }
} while (hasCrafted);
```

---

## 合成规则

由 `ItemConfig.GetNextLevelItem()` 决定：

```csharp
// 计算下一级物品ID: xxx_Ln → xxx_L(n+1)
int baseId = ((int)itemType / 1000) * 1000;
int nextLevel = data.level + 1;
int nextItemId = baseId + nextLevel;
```

**示例**:
- 净水(L1) × 2 → 净水片(L2)
- 净水片(L2) × 2 → 简易净水器(L3)
- ...
- 净水工厂(L9) × 2 → 永动供水站(L10) (需要永恒能源核心)

---

## 满格反馈

当没有空格子放置合成结果时：
1. 触发 `OnFullGridFeedback`
2. 触发 `OnCraftFailure` ("没有空格子放置合成结果")
3. UI 层应播放震动 + 音效 + 格子闪烁

---

## 跨线合成 (设计规范)

部分 L10 物品需要跨线合成：

| 物品 | 合成公式 |
|------|----------|
| 永动供水站 | 净水工厂 + 永恒能源核心 |
| 永动食物机 | 食物合成机 + 永恒能源核心 |
| 自动化工坊 | 智能工厂 + 永恒能源核心 |
| 末日堡垒 | 避难所 + 自动化工坊 |
| 全自动医疗舱 | 医疗中心 + 智能工厂 |
| 文明复兴中心 | 科研中心 + 永恒能源核心 |
| 基因库 | 植物园 + 科研中心 |
| 全息探测仪 | 卫星通讯 + 科研中心 |
