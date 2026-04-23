# 第一层 + 第二层：状态层与事件总线层

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/Core/`

---

## 目录

- [GameState - 纯数据状态根](#gamestate)
- [CellState - 格子状态](#cellstate)
- [PlayerState - 玩家状态](#playerstate)
- [StateMutator - 状态修改器](#statemutator)
- [EventBus - 事件总线](#eventbus)
- [GameEvent - 事件基类](#gameevent)
- [核心事件清单](#核心事件清单)
- [GameEntry - 场景入口](#gameentry)
- [GameLoop - 主循环单例](#gameloop)

---

## GameState

**文件**: `Core/GameState.cs` | **命名空间**: `SaveWorld.Game.Core`

### 职责
游戏状态的纯数据根对象，不可变（Immutable）。所有字段都是 `readonly`，任何修改都必须创建新实例。

### 字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `Version` | `int` | 状态版本号，每次变更+1 |
| `Cells` | `CellState[]` | 63个格子状态数组 |
| `Player` | `PlayerState` | 玩家状态结构体 |
| `Orders` | `IReadOnlyDictionary<int, OrderData>` | 订单字典 |
| `LastOrderResetDate` | `DateTime` | 上次订单重置日期 |
| `Achievements` | `IReadOnlyDictionary<int, AchievementData>` | 成就字典 |
| `Metadata` | `IReadOnlyDictionary<string, object>` | 元数据扩展字典 |

### 方法

```csharp
public static GameState CreateInitial()
public int CalculateOfflineRecoveredStamina(int maxStamina, int recoverIntervalSeconds)
```

---

## CellState

**文件**: `Core/GameState.cs` | **类型**: `readonly struct`

### 字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `Index` | `int` | 格子索引 (0-62) |
| `ItemId` | `int` | 物品ID (0=空) |
| `Count` | `int` | 堆叠数量 |
| `IsLocked` | `bool` | 是否锁定 |

### 工厂方法

```csharp
public static CellState Empty(int index)           // 创建空格
public static CellState Create(int index, int itemId, int count)  // 创建有物品的格
```

### 辅助方法

```csharp
public bool IsEmpty()   // ItemId == 0
public bool HasItem()   // ItemId > 0
```

---

## PlayerState

**文件**: `Core/GameState.cs` | **类型**: `readonly struct`

### 字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `Level` | `int` | 玩家等级 |
| `Stamina` | `int` | 当前体力 |
| `MaxStamina` | `int` | 体力上限 |
| `Gold` | `long` | 金币 |
| `Coins` | `long` | 代币 |
| `Exp` | `int` | 经验值 |
| `Experience` | `int` | 经验值(别名) |
| `ExpToNextLevel` | `int` | 下一级所需经验 |
| `LastOfflineTime` | `long` | 最后离线时间戳 |

### 初始值

```csharp
Level = 1, Stamina = 20, MaxStamina = 100, Gold = 0, Coins = 0
Exp = 0, Experience = 0, ExpToNextLevel = 100
LastOfflineTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
```

---

## StateMutator

**文件**: `Core/StateMutator.cs` | **命名空间**: `SaveWorld.Game.Core`

### 职责
唯一允许修改 `GameState` 的地方。所有状态修改通过监听事件 → 创建新状态副本 → 原子替换 的流程完成。

### 核心机制

```
事件到达
  → 克隆当前 Cells / Player
  → 应用变更逻辑
  → 创建新的 GameState (Version + 1)
  → 替换 _currentState
  → 检查自动保存 (5分钟间隔)
```

### 字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `_eventBus` | `EventBus` | 事件总线引用 |
| `_currentState` | `GameState` | 当前状态 |
| `_storageSystem` | `StorageSystem` | 存储系统引用 |
| `_lastAutoSaveTime` | `DateTime` | 上次自动保存时间 |
| `AUTO_SAVE_INTERVAL_SECONDS` | `const int` | 300 (5分钟) |

### 公开属性

```csharp
public GameState CurrentState => _currentState;
```

### 核心方法

```csharp
// 构造函数 - 注册默认事件处理器
public StateMutator(EventBus eventBus, GameState initialState, StorageSystem storageSystem)

// 立即保存当前状态
public void SaveCurrentState()

// 加载存档状态（含离线体力恢复计算）
public bool LoadSavedState()
```

### 内部状态更新重载

```csharp
// 基础更新：格子 + 玩家
internal void UpdateState(CellState[] newCells, PlayerState newPlayer)

// 含订单更新
internal void UpdateState(CellState[] newCells, PlayerState newPlayer, IReadOnlyDictionary<int, OrderData> newOrders)

// 含成就更新
internal void UpdateState(CellState[] cells, PlayerState player, IReadOnlyDictionary<int, OrderData> orders, IReadOnlyDictionary<int, AchievementData> achievements)

// 全字段更新
internal void UpdateState(CellState[] cells, PlayerState player, IReadOnlyDictionary<int, OrderData> orders, DateTime lastOrderResetDate, IReadOnlyDictionary<int, AchievementData> achievements)
```

### 事件处理器清单

| 事件 | 处理器 | 行为 |
|------|--------|------|
| `MergeCompleteEvent` | `OnMergeComplete` | 清除旧格，创建新物品格 |
| `ItemMovedEvent` | `OnItemMoved` | 移动物品到新格子 |
| `ItemSwappedEvent` | `OnItemSwapped` | 交换两个格子物品 |
| `ExplorationCompleteEvent` | `OnExplorationComplete` | 在空格放置探索获得的物品 |
| `LevelUpEvent` | `OnLevelUp` | 更新玩家状态 |
| `OrderSubmittedEvent` | `OnOrderSubmitted` | 更新玩家状态 |

---

## EventBus

**文件**: `Core/EventBus.cs` | **命名空间**: `SaveWorld.Game.Core`

### 职责
全局唯一事件分发中心。所有跨层通信的唯一通道。

### 字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `_handlers` | `Dictionary<Type, List<Delegate>>` | 事件类型 → 处理器列表 |
| `_eventQueue` | `Queue<GameEvent>` | 事件队列 |

### 核心 API

```csharp
// 订阅事件
public void Listen<T>(Action<T> handler) where T : GameEvent

// 发布事件（加入队列）
public void Publish<T>(T eventData) where T : GameEvent

// 发布事件（别名）
public void Dispatch<T>(T eventData) where T : GameEvent

// 取消订阅
public void Unsubscribe<T>(Action<T> handler) where T : GameEvent

// 处理所有待处理事件（每帧调用）
public void ProcessEvents()

// 清空所有订阅
public void Clear()
```

### 使用示例

```csharp
// 订阅
_eventBus.Listen<MergeCompleteEvent>(e => {
    Debug.Log($"合成完成: {e.NewItemId}");
});

// 发布
_eventBus.Publish(new MergeCompleteEvent(cellId, oldItemId, newItemId, multiplier));
```

---

## GameEvent

**文件**: `Core/EventBus.cs` | **命名空间**: `SaveWorld.Game.Core`

### 定义

```csharp
public abstract class GameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
```

### 通用事件（定义在 EventBus.cs）

| 事件类 | 字段 | 说明 |
|--------|------|------|
| `StaminaClaimedEvent` | `Amount`, `NewStamina` | 体力领取 |
| `CloudSyncStartedEvent` | `SyncId` | 云同步开始 |
| `CloudSyncCompletedEvent` | `Success`, `Message` | 云同步完成 |
| `SFXPlayedEvent` | `SoundId`, `Volume` | 音效播放 |

---

## 核心事件清单

**文件**: `Core/CoreEvents.cs`

### UI 事件

| 事件类 | 字段 | 触发场景 |
|--------|------|----------|
| `CellClickEvent` | `CellId` | 单击格子 |
| `CellDoubleClickEvent` | `CellId` | 双击格子 |
| `CellDragStartEvent` | `CellId` | 开始拖拽格子 |
| `CellDragEndEvent` | `CellId` | 结束拖拽格子 |
| `StaminaRecoverEvent` | `Amount` | 体力恢复 |

### 游戏事件

| 事件类 | 字段 | 触发场景 |
|--------|------|----------|
| `ItemDragStartEvent` | `CellId` | 物品拖拽开始 |
| `ItemDropEvent` | `SourceCellId`, `TargetCellId` | 物品放置 |
| `MergeRequestEvent` | `CellIdA`, `CellIdB`, `ResultItemId`, `RewardMultiplier` | 合成请求 |
| `ExplorationRequestEvent` | `StaminaCost` | 探索请求 |
| `MergeCompleteEvent` | `CellId`, `OldItemId`, `NewItemId`, `RewardMultiplier` | 合成完成 |
| `ItemMovedEvent` | `FromCellId`, `ToCellId`, `ItemId` | 物品移动 |
| `ItemSwappedEvent` | `CellIdA`, `CellIdB`, `ItemIdA`, `ItemIdB` | 物品交换 |
| `ExplorationCompleteEvent` | `GeneratedCellIds[]`, `StaminaUsed` | 探索完成 |
| `OrderSubmittedEvent` | `OrderId`, `RewardGold`, `RewardExp` | 订单提交 |
| `LevelUpEvent` | `OldLevel`, `NewLevel` | 升级 |
| `ItemCraftedEvent` | `ItemType` | 物品合成 |
| `ExperienceGainedEvent` | `Amount`, `Source` | 获得经验 |
| `AchievementUnlockedEvent` | `AchievementId` | 成就解锁 |

---

## GameEntry

**文件**: `Core/GameEntry.cs` | **类型**: `MonoBehaviour`

### 职责
Unity 场景入口脚本，挂载在场景中。负责初始化 `GameLoop` 和处理调试输入。

### 调试快捷键

| 按键 | 功能 |
|------|------|
| `G` | 打印格子信息 |
| `P` | 打印玩家信息 |
| `E` | 测试事件（发布 MergeCompleteEvent） |
| `X` | 执行探索（发布 ExplorationRequestEvent） |

---

## GameLoop

**文件**: `UnityHost/GameLoop.cs` | **类型**: `MonoBehaviour` (单例)

### 职责
游戏主循环入口，所有系统的根。单例模式，跨场景不销毁。

### 公开属性

```csharp
public static GameLoop Instance { get; private set; }
public EventBus EventBus { get; private set; }
public GameState CurrentState { get; private set; }
public StateMutator StateMutator { get; private set; }
```

### 初始化流程 (Awake)

```
1. 单例检查 + DontDestroyOnLoad
2. EventBus = new EventBus()
3. CurrentState = GameState.CreateInitial()
4. StorageSystem = new StorageSystem()
5. StateMutator = new StateMutator(EventBus, CurrentState, StorageSystem)
```

### 每帧流程 (Update)

```csharp
EventBus.ProcessEvents();  // 处理所有待处理事件
```

---

## 状态变更完整链路示例

```
[玩家操作] 双击格子
    ↓
[UI层] UICell.OnCellClicked() → 检测双击 → 发布 CellDoubleClickEvent
    ↓
[行为层] (某系统监听) → 调用 CraftingEngine.TryDoubleTapCraft()
    ↓
[行为层] CraftingEngine 验证规则 → 操作 GridManager → 发布 MergeCompleteEvent
    ↓
[事件层] EventBus 将事件加入队列
    ↓
[事件层] Update() 中 ProcessEvents() 分发事件
    ↓
[状态层] StateMutator.OnMergeComplete() → 克隆 Cells → 修改 → 创建新 GameState
    ↓
[状态层] 自动保存检查
    ↓
[事件层] (UI系统监听状态相关事件) → 刷新界面
```
