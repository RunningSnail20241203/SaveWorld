# 订单系统 (Order System)

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/Order/OrderEngine.cs`

---

## 概述

每日自动生成 5 个订单，玩家提交背包中对应物品完成订单，获得金币和经验奖励。

V2 版本采用纯函数设计。

---

## OrderEngine

**文件**: `Order/OrderEngine.cs` | **命名空间**: `SaveWorld.Game.Order`

### 职责
- 验证订单提交条件
- 生成新订单
- 纯函数计算订单操作结果

### 常量

| 常量 | 值 | 说明 |
|------|-----|------|
| `MAX_ACTIVE_ORDERS` | 3 | 最大活跃订单数 |
| `ORDER_EXPIRE_HOURS` | 6 | 订单过期时间 |

### 核心方法

```csharp
// 尝试提交订单 - 纯函数
public static OrderResult TrySubmitOrder(GameState state, int orderId)

// 生成新订单
public static OrderData GenerateOrder(int playerLevel, int randomSeed)

// 查找订单
private static OrderData? FindOrder(GameState state, int orderId)

// 在背包中查找物品
private static int FindItemInBackpack(GameState state, int itemId)
```

### 提交订单流程

```
1. 查找订单是否存在
2. 检查订单是否已完成
3. 检查订单是否过期（6小时）
4. 检查背包是否有需要的物品
5. 返回 OrderResult (成功/失败原因)
```

### 订单生成规则

```
物品等级 = min(玩家等级 / 10 + 1, 10)
物品ID = random(1, 9)  // 9种基础物品
基础经验 = 20 + 玩家等级 × 5
基础金币 = 10 + 玩家等级 × 3
```

---

## OrderData

```csharp
public readonly struct OrderData
{
    public int OrderId { get; }          // 订单ID
    public int RequireItemId { get; }    // 需求物品ID
    public ItemType RequireItem => (ItemType)RequireItemId;
    public int RewardExp { get; }        // 经验奖励
    public int RewardGold { get; }       // 金币奖励
    public long CreateTime { get; }      // 创建时间戳
    public long ExpireTime { get; }      // 过期时间戳
    public bool IsCompleted { get; }     // 是否已完成
    public bool IsClaimed { get; }       // 是否已领取
}
```

---

## OrderResult

```csharp
public readonly struct OrderResult
{
    public bool Success { get; }
    public int OrderId { get; }
    public int ConsumedCellId { get; }   // 消耗物品的格子索引
    public int RewardGold { get; }
    public int RewardExp { get; }
    public string FailReason { get; }
}
```

### 失败原因

- "订单不存在"
- "订单已完成"
- "订单已过期"
- "背包中没有需要的物品"

---

## 跨天重置

由 `StateMutator` 管理：
- 每天自动重置订单列表
- 生成 5 个新订单
- 记录 `LastOrderResetDate`

---

## 设计要点

1. **纯函数验证**: `TrySubmitOrder` 不修改状态，只返回结果
2. **6小时过期**: 订单有有效期，防止堆积
3. **背包消耗**: 提交时从背包直接扣除物品
4. **等级加权**: 高等级玩家获得更高奖励
