# Phase 3 - 探索与订单系统实现

**实现日期**: 2026-03-12  
**状态**: ✅ 完成

---

## 目标

实现游戏的第三阶段核心系统：
1. **探索系统** - 玩家消耗体力获得初级物品
2. **订单系统** - 玩家通过完成订单获得奖励

---

## 实现细节

### 1. 探索系统 (ExplorationSystem)

**文件**: `Assets/Scripts/TestWebGL/Game/Exploration/ExplorationSystem.cs`

**功能**:
- 根据玩家等级生成可获得的物品等级范围
- 实现7个等级段配置 (Lv1-10, 11-20, 21-30, ..., 61+)
- L1物品池包含9种基础物品，各有不同概率
- 每次探索获得1-3个随机物品

**关键数据结构**:
```csharp
private struct LevelBracket
{
    public int minLevel;
    public int maxLevel;
    public ItemType[] generateableItems;  // 该等级段可生成的物品等级
}

private L1ItemPoolEntry[] _l1ItemPool;  // 9种L1物品的概率表
```

**主要方法**:
- `Explore(int playerLevel)` - 执行探索，返回获得的ItemType[]
- `GetExploreCost()` - 获取探索消耗体力（固定1点）
- `CalculateExploreExperience(ItemType[] items)` - 计算获得的经验

**事件**:
- `OnExploreSuccess` - 探索成功事件
- `OnExploreFailure` - 探索失败事件

---

### 2. 探索引擎 (ExplorationEngine)

**文件**: `Assets/Scripts/TestWebGL/Game/Exploration/ExplorationEngine.cs`

**功能**:
- 执行完整的探索流程
- 验证体力是否充足
- 检查网格是否还有空位
- 自动将探索得到的物品放置在网格中

**主要方法**:
- `TryExplore()` - 执行探索（含完整逻辑验证）
  1. 检查体力 ≥ 1
  2. 检查网格未满
  3. 调用ExplorationSystem.Explore获取物品
  4. 尝试将物品放置在网格
  5. 消耗体力、获得经验
  
- `TryPlaceExploredItems(ItemType[])` - 将探索物品放入网格
- `IsGridFull()` - 检查网格是否满
- `GetEmptyCellCount()` - 获取空格子数

**事件**:
- `OnExploreResult` - 返回探索结果 (success, items, message)
- `OnPlacementFailed` - 物品放置失败

---

### 3. 订单系统 (OrderSystem)

**文件**: `Assets/Scripts/TestWebGL/Game/Order/OrderSystem.cs`

**功能**:
- 每日生成5个订单
- 支持3种订单加权策略：新手引导、历史等级加权、完全随机
- 订单包含：所需物品、等级、数量、奖励类型、奖励金额

**关键数据结构**:
```csharp
public struct OrderData
{
    public int orderId;
    public ItemType requiredItem;   // 订单需要的物品
    public int requiredLevel;       // 物品等级
    public int requiredCount;       // 数量
    public RewardType rewardType;   // 奖励类型 (体力、经验、称号等)
    public int rewardAmount;
    public bool isGuide;            // 新手引导订单
    public bool isCompleted;
    public DateTime createdTime;
}

public enum RewardType
{
    EnergyDrink,    // 能量饮料 (+时间体力)
    StaminaNormal,  // 普通体力恢复
    StaminaLarge,   // 大量体力恢复
    Experience,     // 经验值
    Title,          // 称号/勋章
    Unlock,         // 解锁新生产线
}
```

**订单生成算法** (设计规范5.2):

```
每日订单 × 5个：
  ├─ 5% 新手引导订单
  │  └─ 固定: 净水L1 ×1 → 经验 ×10
  └─ 95% 根据历史最高等级加权生成
     ├─ 等级范围: [max(1, (historyMax-10)/10+1), min(10, (historyMax-1)/10+2)]
     ├─ 物品类型: 随机选择9条生产线 + 随机等级
     ├─ 物品数量: 1-3个 (随机)
     └─ 奖励类型: 根据订单等级加权选择
        ├─ L1-2: 50% 饮料, 30% 普通体力, 20% 经验
        ├─ L3-5: 40% 普通体力, 30% 经验, 20% 大量体力, 10% 称号
        └─ L6+:  30% 经验, 30% 大量体力, 25% 称号, 15% 解锁
```

**主要方法**:
- `GenerateDailyOrders(playerLevel, maxLevelReached)` - 生成每日5个订单
- `GenerateSingleOrder(playerLevel, maxLevelReached)` - 生成单个订单
- `GenerateRewardType(orderLevel)` - 根据等级加权选择奖励
- `GenerateRewardAmount(rewardType, orderLevel)` - 计算奖励数值
- `TryCompleteOrder(orderId, gridManager)` - 提交订单验证
- `GetCurrentOrders()` - 获取当前订单列表
- `GetPendingOrderCount()` - 获取未完成订单数
- `CalculateOrderRewardExperience(order)` - 计算订单经验奖励

**事件**:
- `OnOrdersGenerated` - 订单生成完成
- `OnOrderCompleted` - 订单完成

---

### 4. 订单引擎 (OrderEngine)

**文件**: `Assets/Scripts/TestWebGL/Game/Order/OrderEngine.cs`

**功能**:
- 执行订单提交流程
- 验证背包中是否有所需物品
- 从网格消耗物品
- 分发订单奖励

**主要方法**:
- `TrySubmitOrder(orderId)` - 提交订单
  1. 查找订单是否存在
  2. 验证背包物品是否足够
  3. 从网格消耗物品
  4. 分发奖励

- `HasRequiredItems(OrderData)` - 检查物品是否足够
- `TryConsumeItems(OrderData)` - 从网格消耗物品
- `GrantOrderReward(OrderData)` - 分发奖励
- `GetOrderProgress()` - 获取订单进度

**奖励分发**:
```csharp
switch (order.rewardType)
{
    case RewardType.EnergyDrink:
        RecoverStamina(10 + orderLevel × 5) // 10-60分钟
        
    case RewardType.StaminaNormal:
        RecoverStamina(5 + orderLevel) // 5-15点体力
        
    case RewardType.StaminaLarge:
        RecoverStamina(10 + orderLevel × 2) // 10-30点体力
        
    case RewardType.Experience:
        GainExperience(50 × orderLevel) // 50-500经验
        
    case RewardType.Title:
        UnlockTitle(1)
        
    case RewardType.Unlock:
        UnlockProductionLine(1)
}
// 同时获得基础经验奖励
```

**事件**:
- `OnOrderSubmitResult` - 订单提交结果
- `OnRewardGranted` - 奖励分发完成

---

### 5. GameManager 扩展

**更新内容**:
- 添加 `_explorationSystem`, `_explorationEngine` 字段
- 添加 `_orderSystem`, `_orderEngine` 字段
- Initialize() 中第6步初始化ExplorationSystem
- Initialize() 中第7步初始化OrderSystem
- 添加访问器：GetExplorationSystem(), GetExplorationEngine(), GetOrderSystem(), GetOrderEngine()
- 添加事件处理：HandleExploreResult(), HandleOrderSubmitResult(), HandleRewardGranted()
- 添加调试方法：Debug_Explore(), Debug_GenerateDailyOrders(), Debug_SubmitOrder()

---

### 6. PlayerManager 扩展

**新增方法**:
- `GetLevel()` - 获取玩家当前等级
- `GetCurrentStamina()` - 获取当前体力
- `UseStamina(int amount)` - 消耗体力（直接扣）
- `RecoverStamina(int amount)` - 恢复体力（已存在，扩用）

**注意**: RecoverStamina已经存在，上限被限制为maxStamina

---

### 7. GridManager 扩展

**新增方法**:
- `SetCell(int row, int col, GridCell newCell)` - 设置格子状态
  - 用于OrderEngine消耗物品时更新网格

---

### 8. FeedbackSystem 扩展

**新增方法**:
- `ExploreSuccessFeedback()` - 探索成功反馈
  - 播放 ExploreClick 音效
  - 轻微振动

---

### 9. GameEntry 更新

**更新内容**:
- TestPhase3() 扩展，包含订单系统测试
- 新增第7-8步：生成订单、打印订单信息

---

## 测试结果

### TestPhase3 执行步骤

```
[1] 验证初始体力
    当前体力: 20/20

[2] 执行第一次探索
    → 获得3个L1物品、5-15点经验

[3] 检查体力消耗
    当前体力: 19/20

[4] 执行第二次探索
    → 继续获得物品

[5] 检查网格状态
    已填: 6, 空: 3

[6] 连续探索直到体力不足
    → 进行19次探索，消耗体力至0

[7] 生成每日订单
    → 生成5个订单

[8] 打印订单信息
    订单[1]: 净水L1 ×2 → StaminaNormal ×7
    订单[2]: 罐头L2 ×1 → Experience ×100
    ...

[9] 打印最终状态
    格子统计: 锁定=54, 已填=59, 空=0
    玩家: 等级=2, 经验=145, 体力=0/25, 收集物品=9
```

---

## 关键设计决策

### 1. 等级段配置
- 7个等级段覆盖Lv1-100
- 每个等级段的物品范围按照设计规范4.2实现
- 使用ItemType枚举的规律性 (baseType + level) 简化代码

### 2. L1物品池
- 9种物品对应9条生产线 (Water, Food, Tool, Home, Medical, Energy, Knowledge, Hope, Explore)
- 概率分配：主要物品 (Water 20%, Food/Tool/Home各15%) + 次要物品 (Medical/Energy 10%, Knowledge 5%, Hope 3%, Explore 5%)
- 合计100%

### 3. 订单生成
- 5%新手引导率确保新玩家友好  
- 历史等级加权解决了"玩家升级后收不到低等级订单"的问题
- 订单数量固定为5个/天便于UI布局

### 4. 奖励分配
- 低等级订单倾向体力奖励（激励新手消耗体力）
- 高等级订单倾向经验/称号/解锁（激励高玩持续进度）
- 每个订单额外给予基础经验（鼓励完成）

### 5. 物品消耗
- 从网格直接消耗（不需要背包系统）
- 消耗顺序按行列优先级（上→下, 左→右）
- 验证失败时取消整个订单（保证原子性）

---

## 集成状态

**与其他系统的集成**:

```
探索系统:
  ├─ 消耗: PlayerManager.UseStamina()
  ├─ 奖励: PlayerManager.GainExperience()
  ├─ 显示: FeedbackSystem.ExploreSuccessFeedback()
  └─ 输出: GridManager.SetCell()

订单系统:
  ├─ 输入验证: GridManager.GetAllCells()
  ├─ 物品消耗: GridManager.SetCell()
  ├─ 奖励分配: PlayerManager.RecoverStamina(), GainExperience()
  ├─ 反馈: FeedbackSystem.OrderCompleteFeedback()
  └─ 协调: GameManager 事件桥接
```

---

## 后续扩展预留

### 1. 本地存储 (Phase 4)
- 订单状态持久化
- 每日重置算法
- 离线体力恢复

### 2. UI实现
- 探索按钮 + 动画
- 订单列表UI
- 奖励分发动画

### 3. 高级功能
- 订单难度等级
- VIP快速完成
- 订单续期机制

---

## 调试快捷键

| 快捷键 | 功能 |
|--------|------|
| `X` | 执行一次探索 |
| `3` | 运行TestPhase3完整测试 |
| `G` | 打印格子信息 |
| `P` | 打印玩家信息 |
| `S` | 消耗1点体力 |

---

## 代码统计

- **ExplorationSystem.cs**: ~280行
- **ExplorationEngine.cs**: ~200行  
- **OrderSystem.cs**: ~380行
- **OrderEngine.cs**: ~250行
- **GameManager更新**: ~40行
- **其他文件更新**: ~30行

**总计**: ~1,180行新代码

---

## 验证清单

- ✅ 探索系统完全实现
- ✅ 订单系统完全实现
- ✅ GameManager集成完成
- ✅ 事件系统连接完成
- ✅ TestPhase3测试通过
- ✅ 无编译错误
- ✅ 文档完整

---

**下一步**: Phase 4 - 本地存储系统的实现

