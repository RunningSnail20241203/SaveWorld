# AGENTS.md - 文档索引

**最后更新**: 2026-04-23
**文档版本**: v2.2

> 项目详细文档均位于 `docs/` 目录。需要查什么，先来这里定位文档。

---

## 快速查询指南

- **想知道某个类的字段和方法** → 查对应系统文档
- **想知道事件定义和触发时机** → 查 `docs/core-layer.md` 的「核心事件清单」
- **想知道合成公式** → 查 `docs/item-config.md` 的「合成线完整列表」
- **想知道数据流怎么走** → 查 `docs/architecture.md` 的「关键数据流」
- **想知道某个系统是否完成** → 查对应文档顶部状态标记或 `docs/other-systems.md`
- **想修改某个功能** → 先查对应文档了解职责和API，再定位代码文件

---

## 文档列表

### 架构文档

| 文档 | 路径 | 内容 |
|------|------|------|
| 架构总览 | `docs/architecture.md` | 三层洋葱架构全景、命名空间、目录映射、关键数据流 |
| 状态层与事件层 | `docs/core-layer.md` | GameState、StateMutator、EventBus、所有事件定义、GameLoop、GameEntry |

### 核心系统文档

| 文档 | 路径 | 内容 |
|------|------|------|
| 格子系统 | `docs/grid-system.md` | GridManager、GridCell、锁格配置、错误码、辅助数据结构 |
| 合成引擎 | `docs/crafting-system.md` | CraftingEngine、双击/拖拽/解锁/一键合成流程、跨线合成表 |
| 探索系统 | `docs/exploration-system.md` | ExplorationEngine(V2纯函数)、ExplorationSystem(V1)、物品概率池、经验公式 |
| 订单系统 | `docs/order-system.md` | OrderEngine、OrderData、OrderResult、提交流程、生成规则、跨天重置 |
| 物品配置 | `docs/item-config.md` | ItemType(116种)、ID编码规则、9条合成线完整列表、跨线合成汇总、ItemConfig API |
| 玩家系统 | `docs/player-system.md` | PlayerManager、PlayerData、等级经验配置、体力成长表、经验来源、离线恢复 |
| 存储系统 | `docs/storage-system.md` | StorageSystem、存档数据结构、StorageResult、自动保存、CloudStorageSystem |
| UI系统 | `docs/ui-system.md` | UIManager、BackpackUI、UICell、UIPanelBase、交互设计、UI事件 |
| 微信系统 | `docs/wechat-system.md` | WeChatManager、7个子系统、微信事件定义、接入状态 |
| 其他系统 | `docs/other-systems.md` | 成就、音频、数据分析、社交、反馈、本地化系统的API汇总与状态 |

### 开发文档

| 文档 | 路径 | 内容 |
|------|------|------|
| 开发阶段日志 | `docs/development-log.md` | Phase 1-7 各阶段关键实现、技术决策、代码统计 |

### 工具文档

| 文档 | 路径 | 内容 |
|------|------|------|
| AI-Asset-Generator | `docs/ai-asset-generator.md` | 游戏AI资产生成工具 (图片/音效/音乐)：命令用法、配置、API接入 |
