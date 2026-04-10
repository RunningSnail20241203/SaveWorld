# AGENTS.md - 《末世生存合成》项目指南

## 项目概述

**项目名称**: 末世生存合成 (SaveWorld)  
**项目类型**: Unity微信小游戏  
**开发引擎**: Unity 2022.3 LTS  
**目标平台**: 微信小游戏  
**项目状态**: 🚧 V2重构进行中 (rewrite-v2 分支)  
**架构版本**: 四层洋葱架构 v2.0

---

## 🎯 V2 重构计划

### 重构路线图 13天计划
| 阶段 | 时间 | 内容 |
|---|---|---|
| 第0天 | 1天 | 核心三层基础架构实现 |
| 第1-3天 | 3天 | 核心玩法层实现 |
| 第4-6天 | 3天 | 外围系统实现 |
| 第7-8天 | 2天 | UI与动画表现 |
| 第9-11天 | 3天 | 技术基础设施 |
| 第12-13天 | 2天 | 测试与上线准备 |

### 当前进度
- ✅ 所有设计与架构讨论完成
- ✅ rewrite-v2 分支创建完成
- ⏳ 第0阶段: 核心架构实现中

---

## 🏗️ 四层洋葱架构 (v2.0)

```
                                            ╔══════════════════════════════╗
                                            ║  第四层: 机制层              ║
                                            ║  蝴蝶 垃圾桶 冰冻 活动 PVP    ║
                                            ╚══════════════════════════════╝
                                                  ↓ 监听/取消/修改事件
                                            ╔══════════════════════════════╗
                                            ║  第三层: 事件管道层          ║
                                            ║  全局事件总线 优先级拦截器    ║
                                            ╚══════════════════════════════╝
                                                  ↓ 事件执行
                                            ╔══════════════════════════════╗
                                            ║  第二层: 行为层              ║
                                            ║  移动 合成 探索 提交 升级      ║
                                            ╚══════════════════════════════╝
                                                  ↓ 修改状态
                                            ╔══════════════════════════════╗
                                            ║  第一层: 状态层              ║
                                            ║  不可变游戏状态 纯数据        ║
                                            ╚══════════════════════════════╝
```

### 架构铁律
1.  依赖方向永远只能向内，外层知道内层，内层绝对不知道外层
2.  跨层通信只有事件一种方式，没有任何直接调用
3.  机制之间绝对隔离，删除任何机制不影响其他部分
4.  状态层是纯数据，没有任何方法

---

## 原项目信息保留

### 核心系统架构 (v1.0 已完成)
项目采用模块化、事件驱动的架构，主要系统包括：

1. **GameManager** - 全局单例管理器，协调所有子系统
2. **GridSystem** - 63格背包系统 (9行×7列)
3. **PlayerSystem** - 玩家等级、体力、经验系统
4. **CraftingSystem** - 合成系统 (双击合成、拖拽解锁)
5. **ExplorationSystem** - 探索系统 (消耗体力获得物品)
6. **OrderSystem** - 订单系统 (每日自动生成5个订单)
7. **FeedbackSystem** - 反馈系统 (音效、振动)
8. **StorageSystem** - 存储系统 (本地存档)
9. **AchievementSystem** - 成就系统
10. **UISystem** - UI管理系统

### 扩展系统架构
11. **CloudStorageSystem** - 云存储系统
12. **AudioManager** - 音频管理器
13. **AnalyticsSystem** - 数据分析系统
14. **SocialSystem** - 社交系统
15. **OperationsManager** - 运营管理器
16. **CustomerServiceSystem** - 客服系统
17. **EventSystem** - 活动系统
18. **HotUpdateSystem** - 热更新系统

### 微信小游戏API系统
19. **WeChatAPI** - 微信API基础封装
20. **WeChatLoginSystem** - 微信登录授权
21. **WeChatStorageSystem** - 微信存储系统
22. **WeChatShareSystem** - 微信分享系统
23. **WeChatSocialSystem** - 微信社交系统
24. **WeChatPaySystem** - 微信支付系统
25. **WeChatAdSystem** - 微信广告系统
26. **WeChatManager** - 微信管理器统一接口

## 代码组织

### 目录结构
```
Assets/Scripts/TestWebGL/Game/
├── Core/                    # 核心系统
│   ├── GameManager.cs      # 游戏管理器
│   └── GameEntry.cs        # 游戏入口
├── Grid/                    # 格子系统
│   ├── GridManager.cs      # 格子管理器
│   └── GridCell.cs         # 格子单元
├── Player/                  # 玩家系统
│   └── PlayerManager.cs    # 玩家管理器
├── Crafting/                # 合成系统
│   ├── CraftingSystem.cs   # 合成系统
│   └── CraftingEngine.cs   # 合成引擎
├── Exploration/             # 探索系统
│   ├── ExplorationSystem.cs # 探索系统
│   └── ExplorationEngine.cs # 探索引擎
├── Order/                   # 订单系统
│   ├── OrderSystem.cs      # 订单系统
│   └── OrderEngine.cs      # 订单引擎
├── Storage/                 # 存储系统
│   ├── StorageSystem.cs    # 存储系统
│   └── CloudStorageSystem.cs # 云存储系统
├── Audio/                   # 音频系统
│   └── AudioManager.cs     # 音频管理器
├── Analytics/               # 数据分析
│   └── AnalyticsSystem.cs  # 数据分析系统
├── Social/                  # 社交系统
│   └── SocialSystem.cs     # 社交系统
├── Operations/              # 运营系统
│   ├── OperationsManager.cs # 运营管理器
│   ├── CustomerServiceSystem.cs # 客服系统
│   ├── EventSystem.cs      # 活动系统
│   └── HotUpdateSystem.cs  # 热更新系统
├── WeChat/                  # 微信API
│   ├── WeChatAPI.cs        # 微信API基础
│   ├── WeChatLoginSystem.cs # 登录系统
│   ├── WeChatStorageSystem.cs # 存储系统
│   ├── WeChatShareSystem.cs # 分享系统
│   ├── WeChatSocialSystem.cs # 社交系统
│   ├── WeChatPaySystem.cs  # 支付系统
│   ├── WeChatAdSystem.cs   # 广告系统
│   └── WeChatManager.cs    # 微信管理器
├── UI/                      # UI系统
│   ├── UIManager.cs        # UI管理器
│   ├── GridUI.cs           # 格子UI
│   ├── PlayerInfoPanel.cs  # 玩家信息面板
│   └── ...                 # 其他UI组件
└── Items/                   # 物品系统
    ├── ItemConfig.cs       # 物品配置
    └── ItemIconManager.cs  # 图标管理器
```

## 数据设计

### 物品系统
- **物品总数**: 116种
- **生产线**: 9条（水、食物、工具、住所、医疗、能源、知识、希望、探索）
- **等级范围**: L1-L10
- **跨线合成**: 6种终极物品

### 格子系统
- **总格子数**: 63格 (9行×7列)
- **初始解锁**: 9格 (中心3×3)
- **锁格数量**: 54格
- **堆叠上限**: 99个/格

### 玩家系统
- **等级上限**: 无上限（建议Lv100+）
- **体力上限**: 20-505（随等级增长）
- **体力恢复**: 10分钟-3分钟（随等级加快）

## 开发阶段

### 已完成阶段 (v1.0)
1. **阶段1**: 骨架系统 - 物品、格子、玩家、游戏管理器
2. **阶段2**: 核心玩法 - 合成系统、反馈系统
3. **阶段3**: 外围系统 - 探索、订单、存储
4. **阶段4**: 上线准备 - 云存档、音频、数据分析、社交
5. **阶段5**: 功能完善 - 云存档、音频、数据分析、社交
6. **阶段6**: 上线运营 - 运营、客服、活动、热更新
7. **阶段7**: 微信接入 - 登录、存储、分享、社交、支付、广告

### 当前状态 (v1.0)
- **总代码量**: ~9650行
- **功能完整度**: 100%
- **编译状态**: ✅ 无错误
- **游戏状态**: ✅ 可运行

## 重要文件说明

### 核心文件
- `GameManager.cs` - 游戏全局管理器，协调所有系统
- `GridManager.cs` - 63格背包系统管理器
- `PlayerManager.cs` - 玩家等级、体力、经验管理
- `CraftingEngine.cs` - 合成引擎，处理双击合成和拖拽解锁

### 配置文件
- `ItemConfig.cs` - 116种物品的配置数据
- `UI_RESOURCE_CHECKLIST.md` - UI资源清单
- `游戏设计规范_v1.0_标准格式.md` - 完整游戏设计文档

### 微信相关
- `WeChatManager.cs` - 微信功能统一管理接口
- `WX-WASM-SDK-V2/` - 微信小游戏SDK

## 编码规范

### 命名规范
- 类名: PascalCase (如 `GameManager`)
- 方法名: PascalCase (如 `GetItemName`)
- 变量名: camelCase (如 `currentStamina`)
- 常量: UPPER_SNAKE_CASE (如 `MAX_STAMINA`)
- 私有字段: _camelCase (如 `_playerData`)

### 代码组织
- 每个系统一个命名空间
- 单例模式用于全局管理器
- 事件驱动减少耦合
- 接口定义清晰

### 注释规范
- 类注释: 功能说明
- 方法注释: 参数、返回值、功能
- 复杂逻辑: 行内注释

## 测试与调试

### 调试方法
- Unity控制台输出
- 快捷键测试 (D/G/P/E/S等)
- GameManager调试方法

### 测试覆盖
- 单元测试: 核心逻辑
- 集成测试: 系统交互
- 用户测试: 功能验证

## 部署说明

### 微信小游戏配置
1. 注册微信小游戏账号
2. 配置AppID
3. 设置微信SDK
4. 配置云开发环境

### 构建流程
1. Unity构建WebGL
2. 微信开发者工具上传
3. 提交审核
4. 发布上线

## 维护指南

### 日常维护
- 监控游戏运行状态
- 处理玩家反馈
- 更新活动内容
- 性能优化

### 版本更新
- 热更新系统支持
- 灰度发布
- 回滚机制

## 联系方式

- **项目仓库**: git@github.com:RunningSnail20241203/SaveWorld.git
- **开发团队**: 个人/朋友项目
- **技术支持**: 通过GitHub Issues

---

**最后更新**: 2026-04-09  
**文档版本**: v2.0  
**项目状态**: 🚧 V2重构进行中