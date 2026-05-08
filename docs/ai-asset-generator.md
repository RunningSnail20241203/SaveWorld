# AI Asset Generator 游戏AI资产生成工具

> 文档版本: v2.0 | 最后更新: 2026-05-07
> 对应代码: `AI-Asset-Generator/`

---

## 概述

用于批量生成游戏AI资产（Prompt / 图片 / 音效 / 音乐），当前已支持：Prompt生成、阿里云百炼 Z-Image 文生图。

---

## 目录结构

```
AI-Asset-Generator/
├── package.json            # 项目配置与 npm 脚本
├── config/
│   ├── items.json          # 物品配置（合成线、特殊物品、跨线物品）
│   └── ui.json             # UI 元素配置
├── prompts/
│   ├── items/              # 输出的物品 Prompt 文件 (.txt)
│   └── ui/                 # 输出的 UI Prompt 文件 (.txt)
└── src/
    ├── index.js            # CLI 入口，解析命令行参数
    ├── generator.js        # Prompt 生成核心逻辑
    └── templates.js        # Prompt 模板定义
```

---

## 快速开始

```bash
cd AI-Prompt-Generator
npm install
npm run generate -- [选项]
```

---

## 可用命令

| 命令 | 说明 |
|------|------|
| `npm run generate -- --item <名称>` | 生成单个物品的 Prompt |
| `npm run generate -- --level <1-10>` | 按物品等级批量生成 |
| `npm run generate -- --line <名称>` | 按合成线批量生成 |
| `npm run generate -- --all-items` | 生成所有物品的 Prompt |
| `npm run generate -- --ui` | 生成 UI 资源的 Prompt |
| `npm run generate -- --all` | 生成全部资源（物品 + UI）|
| `npm run generate -- --help` | 显示帮助信息 |
| `npm run generate -- --version` | 显示版本 |

### 命令示例

```bash
# 生成单个物品
npm run generate -- --item 净水

# 按等级生成
npm run generate -- --level 1

# 按合成线生成
npm run generate -- --line 水源

# 生成所有物品
npm run generate -- --all-items

# 生成 UI 资源
npm run generate -- --ui

# 生成全部
npm run generate -- --all
```

---

## 核心模块

### index.js - CLI 入口

**职责**: 解析命令行参数，分发到对应的生成函数。

**解析的选项**:
- `--item`, `-i` → 生成单个物品
- `--level`, `-l` → 按等级批量生成
- `--line`, `-L` → 按合成线批量生成
- `--all-items`, `-a` → 生成所有物品
- `--ui`, `-u` → 生成 UI 资源
- `--all` → 生成全部
- `--help`, `-h` → 帮助
- `--version`, `-v` → 版本

---

### generator.js - 生成核心

**职责**: 加载配置、调用模板、保存文件。

#### 核心方法

```javascript
function init()                              // 初始化输出目录
function generateSingleItem(itemName)        // 生成单个物品 Prompt
function generateByLevel(level)              // 按等级批量生成
function generateByLine(lineName)            // 按合成线批量生成
function generateAllItems()                  // 生成所有物品
function generateUI()                        // 生成 UI Prompt
function generateAll()                       // 生成全部资源
```

#### 配置加载

```javascript
function loadItemsConfig()   // 加载 config/items.json
function loadUIConfig()      // 加载 config/ui.json
```

---

### templates.js - Prompt 模板

**职责**: 定义物品图标和 UI 贴图的 Prompt 模板。

#### 物品图标模板

```javascript
function generateItemPrompt(item, lineTheme)
```

**输出格式**:
```
游戏物品图标, 末世生存主题, {物品名称}, 等级{level}, 
{描述}, 扁平化图标风格, {等级色调}, 
无背景透明PNG格式, 80x80像素, Unity UI Sprite使用, 
{等级风格}, {等级细节}, 简约UI风格, 
清晰的物品轮廓, 适合游戏内使用, 高对比度, 
主题元素: {lineTheme}
```

#### 等级风格修饰表

| 等级 | 风格 | 色调 | 细节 |
|------|------|------|------|
| Lv.1 | 粗糙, 简约, 基础 | 冷色调, 灰暗 | 简单线条 |
| Lv.2 | 简洁, 朴素, 实用 | 冷色调, 暗淡 | 基础细节 |
| Lv.3 | 实用, 改良, 精致 | 中性色调 | 适度细节 |
| Lv.4 | 完善, 精细, 优质 | 中性色调, 偏暖 | 丰富细节 |
| Lv.5 | 专业, 高品质, 完善 | 暖色调, 明亮 | 精细纹理 |
| Lv.6 | 先进, 高科技, 精致 | 暖色调, 明亮 | 科技感 |
| Lv.7 | 尖端, 未来感, 豪华 | 明亮, 发光 | 高级特效 |
| Lv.8 | 超现代, 全息, 科幻 | 发光, 蓝色 | 全息效果 |
| Lv.9 | 终极, 超级, 未来 | 金色, 发光 | 光环特效 |
| Lv.10 | 传奇, 永恒, 神圣 | 金色, 彩虹光 | 神圣光辉 |

#### UI 贴图模板

```javascript
function generateUIPrompt(uiType, options)
```

支持的 `uiType`:
- `button` - 按钮
- `panel` - 面板
- `progressBar` - 进度条
- `background` - 背景
- `icon` - 图标
- `border` - 边框

---

## 配置文件

### items.json

定义物品配置，包括合成线、特殊物品、跨线物品。每个物品包含:
- `id` - 唯一标识
- `name` - 物品名称
- `level` - 等级 (1-10)
- `description` - 描述

### ui.json

定义 UI 元素配置。每个元素包含:
- `type` - UI类型 (button/panel/progressBar/background/icon/border)
- `name` - 名称
- `description` - 描述
- `color` - 颜色（可选）
- `size` - 尺寸（可选）

---

## 输出目录

```
prompts/
├── items/
│   ├── water_1.txt      # 净水 Prompt
│   ├── water_2.txt      # 净水片 Prompt
│   └── ...
└── ui/
    ├── button_主按钮.txt
    ├── panel_面板.txt
    └── ...
```

---

## 设计要点

1. **物品图标**: 根据等级自动切换风格修饰（Lv.1 粗糙灰暗 → Lv.10 传奇神圣）
2. **统一规格**: 80x80 像素、透明背景、Unity UI Sprite 适用
3. **UI 贴图**: 6种类型统一末世生存风格
4. **批量生成**: 支持按物品、等级、合成线、全部多种维度生成
