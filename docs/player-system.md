# 玩家系统 (Player System)

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/Player/PlayerManager.cs`

---

## 概述

管理玩家的等级、经验、体力、图鉴等系统。单例模式，通过事件总线监听游戏事件自动更新。

---

## PlayerManager

**文件**: `Player/PlayerManager.cs` | **命名空间**: `SaveWorld.Game.Player`

### 职责
- 等级与经验管理
- 体力消耗与恢复
- 图鉴收集
- 监听合成/经验事件自动更新

### 单例访问

```csharp
public static PlayerManager Instance { get; }
```

### 核心方法

```csharp
// 初始化
public void Initialize(PlayerData playerData = null)

// 经验
public void GainExperience(int amount, string reason = "")
public int GetExperienceForLevel(int level)           // 获取升到某级所需总经验
public int GetRemainingExpForLevelUp()                // 获取升级所需剩余经验

// 体力
public bool TryUseStamina(int amount)
public void UseStamina(int amount)
public int GetCurrentStamina()
public void RecoverStamina(int amount)
public int CalculateAutoRecoveredStamina()            // 离线恢复计算
public int GetStaminaRecoveryMinutes()                // 当前恢复速度(分钟/点)

// 等级
public int GetLevel()
public int GetHistoryMaxLevel()

// 图鉴
public void RecordItemCollected(ItemType itemType)
public bool IsItemCollected(ItemType itemType)

// 统计
public PlayerStatistics GetStatistics()
```

### 事件

```csharp
public delegate void LevelChangedHandler(int newLevel, int oldLevel);
public event LevelChangedHandler OnLevelChanged;

public delegate void StaminaChangedHandler(int newStamina, int maxStamina);
public event StaminaChangedHandler OnStaminaChanged;

public delegate void ExperienceGainedHandler(int amount, string reason);
public event ExperienceGainedHandler OnExperienceGained;
```

---

## PlayerData

```csharp
[Serializable]
public class PlayerData
{
    public int playerId;
    public string playerName = "玩家";
    public DateTime createdTime;
    public DateTime lastSaveTime;

    public int level = 1;
    public int experience = 0;

    public int currentStamina = 20;
    public int maxStamina = 20;
    public DateTime lastStaminaRecoverTime;

    public int historyMaxLevel = 1;
    public List<int> collectedItems = new List<int>();      // 图鉴
    public List<int> unlockedAchievements = new List<int>(); // 成就

    public int dailyOrderRefreshCount = 0;
    public DateTime lastOrderRefreshTime;
}
```

---

## 等级经验配置

```csharp
private readonly int[] _levelExpRequirement = new int[]
{
    0, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000,   // Lv1-10
    1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000  // Lv11-20
    // 后续按 100 × 等级 递增
};
```

**升到某级所需总经验**: 从 Lv1 累加到目标等级前一级。

---

## 体力成长表

| 等级 | 体力上限 | 恢复速度 |
|------|----------|----------|
| 1 | 20 | 10分钟/点 |
| 10 | 25 | 10分钟/点 |
| 20 | 35 | 9分钟/点 |
| 30 | 55 | 8分钟/点 |
| 40 | 85 | 7分钟/点 |
| 50 | 125 | 6分钟/点 |
| 60 | 175 | 5分钟/点 |
| 70 | 235 | 4分钟/点 |
| 80 | 315 | 4分钟/点 |
| 90 | 405 | 3分钟/点 |
| 100 | 505 | 3分钟/点 |

---

## 经验来源

| 来源 | 公式 |
|------|------|
| 合成 | 产出等级 × 数量 |
| 解锁格子 | 20 × 物品等级 |
| 探索 | 5 × 物品等级 (每个物品) |
| 订单 | 20 + 玩家等级 × 5 |

---

## 离线体力恢复

```
离线分钟数 = 当前时间 - 上次恢复时间
恢复点数 = 离线分钟数 / 恢复速度
实际恢复 = min(恢复点数, 上限 - 当前体力)
```
