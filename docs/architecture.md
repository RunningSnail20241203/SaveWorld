# 🏗️ 三层洋葱架构 v2.0 总览

> 文档版本: v2.2 | 最后更新: 2026-04-23

---

## 架构概览图

```
╔══════════════════════════════════════════════════════════════════════╗
║                        Unity 引擎层                                  ║
║   GameEntry (场景入口)  →  GameLoop (单例主循环)                     ║
╚══════════════════════════════════════════════════════════════════════╝
                              ↓ 初始化
╔══════════════════════════════════════════════════════════════════════╗
║  第三层: 行为层 (Behavior Layer)                                     ║
║  所有业务逻辑和外部交互都在这里                                       ║
║                                                                      ║
║  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   ║
║  │ Crafting    │ │ Exploration │ │ Order       │ │ Grid        │   ║
║  │ Engine      │ │ Engine      │ │ Engine      │ │ Manager     │   ║
║  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   ║
║  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   ║
║  │ Player      │ │ Achievement │ │ Audio       │ │ Analytics   │   ║
║  │ Manager     │ │ System      │ │ Manager     │ │ System      │   ║
║  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   ║
║  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   ║
║  │ Social      │ │ Feedback    │ │ UI          │ │ WeChat      │   ║
║  │ System      │ │ System      │ │ Manager     │ │ Manager     │   ║
║  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   ║
║                                                                      ║
║  行为层 → 产生事件 → EventBus.Publish(event)                         ║
╚══════════════════════════════════════════════════════════════════════╝
                              ↓ 事件队列
╔══════════════════════════════════════════════════════════════════════╗
║  第二层: 事件总线层 (Event Bus Layer)                                ║
║  EventBus - 全局唯一事件分发中心                                      ║
║                                                                      ║
║  职责:                                                               ║
║  • 事件订阅 (Listen<T>)                                              ║
║  • 事件发布 (Publish/Dispatch<T>)                                    ║
║  • 事件队列处理 (ProcessEvents - 每帧调用)                           ║
║  • 事件取消订阅 (Unsubscribe<T>)                                     ║
║                                                                      ║
║  所有跨层通信的唯一通道                                               ║
╚══════════════════════════════════════════════════════════════════════╝
                              ↓ 事件分发
╔══════════════════════════════════════════════════════════════════════╗
║  第一层: 状态层 (State Layer)                                        ║
║  纯数据对象，不可变，无业务逻辑                                       ║
║                                                                      ║
║  ┌─────────────────────────────────────────────┐                     ║
║  │ GameState (版本化不可变状态根对象)           │                     ║
║  │ ├── CellState[63]  格子状态数组              │                     ║
║  │ ├── PlayerState    玩家状态                  │                     ║
║  │ ├── Orders         订单字典                  │                     ║
║  │ ├── Achievements   成就字典                  │                     ║
║  │ └── Metadata       元数据扩展字典            │                     ║
║  └─────────────────────────────────────────────┘                     ║
║                                                                      ║
║  StateMutator - 唯一允许修改 GameState 的地方                         ║
║  • 监听事件 → 计算新状态 → 创建不可变副本 → 替换当前状态             ║
║  • 自动保存检查 (5分钟间隔)                                          ║
║  • 离线体力恢复计算                                                  ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## 架构铁律

1. **依赖方向永远向内**：外层知道内层，内层绝对不知道外层
2. **跨层通信只有事件一种方式**：无直接调用
3. **状态变更统一由 `StateMutator` 负责**
4. **`GameState` 是纯数据，无业务方法**
5. **所有状态变更可通过事件溯源完整重现**

---

## 命名空间规范

| 命名空间 | 层级 | 说明 |
|---------|------|------|
| `SaveWorld.Game.Core` | 内核 | 状态层 + 事件层 + 入口 |
| `SaveWorld.Game.UnityHost` | 宿主 | GameLoop 单例 |
| `SaveWorld.Game.Grid` | 行为层 | 格子系统 |
| `SaveWorld.Game.Crafting` | 行为层 | 合成引擎 |
| `SaveWorld.Game.Exploration` | 行为层 | 探索系统 |
| `SaveWorld.Game.Order` | 行为层 | 订单系统 |
| `SaveWorld.Game.Items` | 行为层 | 物品配置 |
| `SaveWorld.Game.Player` | 行为层 | 玩家系统 |
| `SaveWorld.Game.Storage` | 行为层 | 存储系统 |
| `SaveWorld.Game.UI` | 行为层 | UI系统 |
| `SaveWorld.Game.WeChat` | 行为层 | 微信API |
| `SaveWorld.Game.Achievement` | 行为层 | 成就系统 |
| `SaveWorld.Game.Audio` | 行为层 | 音频系统 |
| `SaveWorld.Game.Analytics` | 行为层 | 数据分析 |
| `SaveWorld.Game.Social` | 行为层 | 社交系统 |
| `SaveWorld.Game.Feedback` | 行为层 | 反馈系统 |
| `SaveWorld.Game.Localization` | 行为层 | 本地化 |

---

## 文件目录映射

```
Assets/Scripts/TestWebGL/Game/
├── Core/                    # 状态层 + 事件层
│   ├── GameState.cs         # 纯数据状态根
│   ├── StateMutator.cs      # 状态修改器
│   ├── EventBus.cs          # 事件总线
│   ├── CoreEvents.cs        # 核心游戏事件定义
│   └── GameEntry.cs         # Unity场景入口
├── UnityHost/
│   └── GameLoop.cs          # 单例主循环
├── Grid/                    # 格子系统
│   ├── GridManager.cs
│   └── GridCell.cs
├── Crafting/                # 合成引擎
│   └── CraftingEngine.cs
├── Exploration/             # 探索系统
│   ├── ExplorationSystem.cs # 旧版（V1）
│   └── ExplorationEngine.cs # V2纯函数版
├── Order/                   # 订单系统
│   └── OrderEngine.cs
├── Items/                   # 物品配置
│   ├── ItemConfig.cs        # 116种物品定义
│   └── ItemIconManager.cs
├── Player/                  # 玩家系统
│   └── PlayerManager.cs
├── Storage/                 # 存储系统
│   ├── StorageSystem.cs     # 本地存储
│   └── CloudStorageSystem.cs # 云存储
├── UI/                      # UI系统
│   ├── UIManager.cs
│   ├── UIPanelBase.cs
│   ├── BackpackUI.cs
│   ├── ExplorationUI.cs
│   ├── OrderUI.cs
│   └── PlayerStatusUI.cs
├── WeChat/                  # 微信API
│   ├── WeChatManager.cs     # 统一入口
│   ├── WeChatAPI.cs
│   ├── WeChatLoginSystem.cs
│   ├── WeChatAdSystem.cs
│   ├── WeChatShareSystem.cs
│   ├── WeChatPaySystem.cs
│   ├── WeChatSocialSystem.cs
│   └── WeChatStorageSystem.cs
├── Achievement/             # 成就系统
│   └── AchievementSystem.cs
├── Audio/                   # 音频系统
│   └── AudioManager.cs
├── Analytics/               # 数据分析
│   └── AnalyticsSystem.cs
├── Social/                  # 社交系统
│   └── SocialSystem.cs
├── Feedback/                # 反馈系统
│   └── FeedbackSystem.cs
└── Localization/            # 本地化
    └── LocalizationManager.cs
```

---

## 关键数据流

### 1. 合成流程
```
玩家双击格子
  → UICell.OnCellClicked() 发布 CellDoubleClickEvent
  → (某行为层监听处理)
  → CraftingEngine.TryDoubleTapCraft() 检查规则
  → 发布 MergeCompleteEvent
  → StateMutator.OnMergeComplete() 修改状态
  → EventBus 分发状态变更事件
  → UIManager 刷新 UI
```

### 2. 探索流程
```
玩家点击探索按钮
  → 发布 ExplorationRequestEvent
  → ExplorationEngine.TryExplore(state, seed) 纯函数计算
  → 返回 ExplorationResult
  → 发布 ExplorationCompleteEvent
  → StateMutator.OnExplorationComplete() 修改状态
  → UI 刷新
```

### 3. 订单提交流程
```
玩家点击提交订单
  → OrderEngine.TrySubmitOrder(state, orderId) 纯函数验证
  → 返回 OrderResult
  → 发布 OrderSubmittedEvent
  → StateMutator.OnOrderSubmitted() 修改状态
  → UI 刷新 + 成就检查
```

---

## 版本历史

| 版本 | 日期 | 说明 |
|------|------|------|
| v2.0 | 2026-04-10 | 三层洋葱架构初始建立 |
| v2.1 | 2026-04-16 | 加入订单、成就、自动保存系统 |
| v2.2 | 2026-04-23 | 完善所有系统框架，生成文档体系 |
