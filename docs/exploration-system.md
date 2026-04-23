# 探索系统 (Exploration System)

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/Exploration/`

---

## 概述

管理玩家的探索行为。消耗体力 → 获得随机物品 → 自动放入背包空格。

V2 版本采用纯函数设计：`ExplorationEngine` 无状态、无副作用、零依赖。

---

## 双版本并存

| 版本 | 文件 | 设计模式 | 状态 |
|------|------|----------|------|
| V1 | `ExplorationSystem.cs` | 单例有状态 | 旧版兼容 |
| V2 | `ExplorationEngine.cs` | 静态纯函数 | 推荐使用 |

---

## ExplorationEngine (V2 推荐)

**文件**: `Exploration/ExplorationEngine.cs` | **命名空间**: `SaveWorld.Game.Exploration`

### 职责
纯函数计算探索结果。输入 `GameState` 快照和随机种子，输出 `ExplorationResult`。

### 核心方法

```csharp
// 尝试探索 - 纯函数
public static ExplorationResult TryExplore(GameState state, int randomSeed)

// 查找空格子 - 优先随机分配
private static int[] FindEmptyCells(GameState state, int count, Random random)

// 根据玩家等级生成随机物品
private static int GenerateRandomItem(int playerLevel, Random random)
```

### 探索流程

```
1. 检查体力 ≥ 1
2. 检查背包有空格
3. 生成 1-3 个物品 (受空格数限制)
4. Fisher-Yates 洗牌分配空格
5. 计算经验 (5 × 物品等级)
6. 返回 ExplorationResult
```

### L1 物品概率池

| 物品 | ID | 概率 |
|------|-----|------|
| 净水 | 1 | 20% |
| 罐头 | 2 | 15% |
| 零件 | 3 | 15% |
| 木材 | 4 | 15% |
| 草药 | 5 | 10% |
| 电池 | 6 | 10% |
| 旧书 | 7 | 5% |
| 种子 | 8 | 3% |
| 地图碎片 | 9 | 5% |

### ExplorationResult

```csharp
public readonly struct ExplorationResult
{
    public bool Success { get; }
    public int[] CellIds { get; }       // 放置的格子索引
    public int[] ItemIds { get; }       // 获得的物品ID
    public int StaminaCost { get; }     // 消耗体力
    public int ExperienceGain { get; }  // 获得经验
    public string FailReason { get; }   // 失败原因
}
```

---

## ExplorationSystem (V1 旧版)

**文件**: `Exploration/ExplorationSystem.cs` | **命名空间**: `SaveWorld.Game.Exploration`

### 职责
有状态的单例，管理探索配置和玩家等级段。

### 等级段配置

| 玩家等级 | 可生成物品等级 |
|----------|---------------|
| Lv1-10 | L1 |
| Lv11-20 | L1, L2 |
| Lv21-30 | L1, L2, L3 |
| Lv31-40 | L2, L3, L4 |
| Lv41-50 | L3, L4, L5 |
| Lv51-60 | L4, L5, L6 |
| Lv61+ | L5, L6, L7 |

### 核心方法

```csharp
public ItemType[] Explore(int playerLevel)        // 执行探索
public bool TryExplore()                          // 检查体力后探索
public int GetExploreCost()                       // 固定消耗 1 体力
public int CalculateExploreExperience(ItemType[] items)  // 计算探索经验
```

---

## 探索经验公式

```
探索经验 = Σ(5 × 物品等级)
```

**示例**: 获得净水(L1) + 罐头(L1) + 零件(L1)
- 经验 = 5×1 + 5×1 + 5×1 = 15

---

## 设计要点

1. **纯函数**: V2 版本 `TryExplore` 不修改任何状态
2. **随机种子**: 支持可复现的随机结果（方便测试和存档同步）
3. **Fisher-Yates 洗牌**: 避免物品总是堆在左上角
4. **空格限制**: 生成数量受限于当前空格数
5. **固定消耗**: 每次探索固定消耗 1 体力
