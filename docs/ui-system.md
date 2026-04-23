# UI 系统 (UI System)

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/UI/`

---

## 概述

监听游戏事件，响应状态变化，更新 Unity UI。所有 UI 更新通过事件总线驱动。

---

## UIManager

**文件**: `UI/UIManager.cs` | **命名空间**: `SaveWorld.Game.UI`

### 职责
- 监听所有游戏事件
- 统一刷新各类 UI
- 管理弹窗显示

### 构造函数

```csharp
public UIManager(EventBus eventBus, StateMutator stateMutator)
```

### 监听事件

| 事件 | 响应 |
|------|------|
| `MergeCompleteEvent` | 刷新格子UI |
| `ItemMovedEvent` | 刷新格子UI |
| `ExplorationCompleteEvent` | 刷新格子UI + 玩家UI |
| `OrderSubmittedEvent` | 刷新格子UI + 玩家UI + 订单UI |
| `AchievementUnlockedEvent` | 显示成就弹窗 |
| `LevelUpEvent` | 显示升级弹窗 |
| `StaminaClaimedEvent` | 刷新玩家UI |
| `CloudSyncStartedEvent` | 显示同步指示器 |
| `CloudSyncCompletedEvent` | 隐藏同步指示器 |
| `SFXPlayedEvent` | 播放按钮动画 |

### 核心方法

```csharp
public void RefreshGridUI()       // 刷新所有格子
public void RefreshPlayerUI()     // 刷新玩家信息
public void RefreshOrdersUI()     // 刷新订单列表
public void ShowAchievementPopup(AchievementUnlockedEvent e)
public void ShowLevelUpPopup(LevelUpEvent e)
```

---

## BackpackUI

**文件**: `UI/BackpackUI.cs` | **类型**: `UIPanelBase`

### 职责
63 格背包的完整 UI 实现，包括点击、双击、拖拽交互。

### 配置

```csharp
[Header("背包配置")]
public GridLayoutGroup GridLayout;
public GameObject CellPrefab;
```

### 交互设计

| 操作 | 触发 |
|------|------|
| 单击 | 发布 `CellClickEvent` |
| 双击 | 发布 `CellDoubleClickEvent` → 触发合成 |
| 长按(0.4s) | 开始拖拽 → 发布 `CellDragStartEvent` |
| 释放 | 发布 `CellDragEndEvent` → 移动/交换 |

### 核心方法

```csharp
public void RefreshAll()                    // 刷新所有格子
public void RefreshCell(int cellId)         // 刷新单个格子
public void PlayMergeAnimation(int cellId)  // 播放合成动画
```

---

## UICell

**文件**: `UI/BackpackUI.cs` (内部类)

### 职责
单个格子的 UI 表现和交互处理。

### 字段

```csharp
public int CellId;
public Image IconImage;
public Text LevelText;
public Button CellButton;
```

### 交互参数

```csharp
private const float DOUBLE_CLICK_THRESHOLD = 0.3f;  // 双击阈值
private const float LONG_PRESS_THRESHOLD = 0.4f;    // 长按阈值
```

---

## UIPanelBase

**文件**: `UI/UIPanelBase.cs`

### 职责
所有 UI 面板的基类，提供通用功能。

```csharp
public abstract class UIPanelBase : MonoBehaviour
{
    public virtual void Initialize() { }
    public virtual void Show() { }
    public virtual void Hide() { }
    public virtual void Refresh() { }
}
```

---

## 其他 UI 面板

| 面板 | 文件 | 职责 |
|------|------|------|
| 探索UI | `ExplorationUI.cs` | 探索按钮、结果展示 |
| 订单UI | `OrderUI.cs` | 订单列表、提交按钮 |
| 玩家状态UI | `PlayerStatusUI.cs` | 等级、体力、金币显示 |

---

## UI 事件定义

```csharp
public class GridUIRefreshedEvent : GameEvent { }
public class PlayerUIRefreshedEvent : GameEvent { }
public class OrdersUIRefreshedEvent : GameEvent { }

public class AchievementPopupShownEvent : GameEvent
{
    public int AchievementId;
    public string AchievementName;
}

public class LevelUpPopupShownEvent : GameEvent
{
    public int NewLevel;
}
```
