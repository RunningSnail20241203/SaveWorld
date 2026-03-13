# UI预制件创建指南

## 概述
由于所有UI组件都是代码动态创建的，需要在Unity Editor中创建对应的预制件以实现可视化设计和更好的维护性。

## 预制件创建步骤

### 1. 创建UI预制件文件夹
在Project窗口中：
```
Assets/
├── Prefabs/
│   └── UI/
│       ├── GridUI.prefab
│       ├── PlayerInfoPanel.prefab
│       ├── ControlPanel.prefab
│       ├── ItemDetailPopup.prefab
│       ├── SettingsPanel.prefab
│       ├── OrdersPanel.prefab
│       ├── AchievementPanel.prefab
│       └── GridCellUI.prefab
```

### 2. 基础设置
所有UI预制件都需要：
- **Canvas** 作为根节点（如果不是面板）
- **RectTransform** 组件
- **Canvas Group** 组件（用于透明度和交互控制）
- 适当的锚点和位置设置

---

## 详细创建指南

### GridUI.prefab (网格UI)
**用途**: 显示9×7的游戏网格

**结构**:
```
GridUI (GameObject)
├── Background (Image) - 背景图片
├── GridContainer (GridLayoutGroup) - 网格容器
│   ├── Cell_0_0 (GridCellUI prefab instance)
│   ├── Cell_0_1 (GridCellUI prefab instance)
│   └── ... (总共63个格子)
└── ScrollRect (ScrollRect) - 可选的滚动区域
```

**组件设置**:
- **GridContainer**:
  - GridLayoutGroup: 7列，9行
  - CellSize: 根据屏幕适配
  - Spacing: 2-5像素

### GridCellUI.prefab (网格单元UI)
**用途**: 单个网格单元的显示

**结构**:
```
GridCellUI (GameObject)
├── Background (Image) - 单元格背景
├── ItemIcon (Image) - 物品图标
├── ItemCount (TextMeshProUGUI) - 物品数量
├── LockIcon (Image) - 锁定图标
└── Highlight (Image) - 高亮边框
```

**组件设置**:
- **Background**: 不同状态使用不同颜色
- **ItemIcon**: 居中显示，保持宽高比
- **ItemCount**: 右下角显示，白色文字

### PlayerInfoPanel.prefab (玩家信息面板)
**用途**: 显示玩家等级、经验、体力等信息

**结构**:
```
PlayerInfoPanel (GameObject)
├── Background (Image)
├── PlayerName (TextMeshProUGUI)
├── LevelText (TextMeshProUGUI)
├── ExperienceBar (Slider)
│   ├── Background (Image)
│   ├── Fill (Image)
│   └── Handle (RectTransform)
├── StaminaBar (Slider)
│   ├── Background (Image)
│   ├── Fill (Image)
│   └── Handle (RectTransform)
└── StatsContainer (VerticalLayoutGroup)
    ├── ExperienceText (TextMeshProUGUI)
    └── StaminaText (TextMeshProUGUI)
```

**组件设置**:
- **ExperienceBar/StaminaBar**: 使用Slider组件显示进度
- **布局**: 使用VerticalLayoutGroup垂直排列

### ControlPanel.prefab (控制面板)
**用途**: 游戏主要操作按钮

**结构**:
```
ControlPanel (GameObject)
├── Background (Image)
└── ButtonContainer (HorizontalLayoutGroup)
    ├── ExploreButton (Button)
    │   ├── Background (Image)
    │   └── Text (TextMeshProUGUI)
    ├── SettingsButton (Button)
    │   ├── Background (Image)
    │   └── Text (TextMeshProUGUI)
    ├── OrdersButton (Button)
    │   ├── Background (Image)
    │   └── Text (TextMeshProUGUI)
    ├── AchievementsButton (Button)
    │   ├── Background (Image)
    │   └── Text (TextMeshProUGUI)
    └── SaveButton (Button)
        ├── Background (Image)
        └── Text (TextMeshProUGUI)
```

**组件设置**:
- **ButtonContainer**: HorizontalLayoutGroup，均匀分布按钮
- **每个按钮**: Button组件 + 背景图片 + 文字

### ItemDetailPopup.prefab (物品详情弹窗)
**用途**: 显示物品详细信息

**结构**:
```
ItemDetailPopup (GameObject)
├── Background (Image)
├── ItemIcon (Image)
├── ItemName (TextMeshProUGUI)
├── ItemDescription (TextMeshProUGUI)
├── ItemStats (VerticalLayoutGroup)
│   ├── LevelText (TextMeshProUGUI)
│   ├── TypeText (TextMeshProUGUI)
│   └── RarityText (TextMeshProUGUI)
├── ActionButton (Button)
│   ├── Background (Image)
│   └── Text (TextMeshProUGUI)
└── CloseButton (Button)
    └── Background (Image)
```

**组件设置**:
- **布局**: 垂直布局，图标在上，信息在下
- **ActionButton**: 根据物品状态显示不同文字

### SettingsPanel.prefab (设置面板)
**用途**: 游戏设置界面

**结构**:
```
SettingsPanel (GameObject)
├── Background (Image)
├── Title (TextMeshProUGUI)
├── SettingsContainer (ScrollRect)
│   └── Content (VerticalLayoutGroup)
│       ├── AudioSection (GameObject)
│       │   ├── AudioTitle (TextMeshProUGUI)
│       │   ├── MasterVolume (Slider + Text)
│       │   ├── MusicVolume (Slider + Text)
│       │   └── SFXVolume (Slider + Text)
│       ├── DisplaySection (GameObject)
│       │   ├── DisplayTitle (TextMeshProUGUI)
│       │   ├── FullscreenToggle (Toggle + Text)
│       │   └── VSyncToggle (Toggle + Text)
│       └── GameSection (GameObject)
│           ├── GameTitle (TextMeshProUGUI)
│           ├── AutoSaveToggle (Toggle + Text)
│           └── ShowTipsToggle (Toggle + Text)
├── ApplyButton (Button)
├── ResetButton (Button)
└── CloseButton (Button)
```

**组件设置**:
- **ScrollRect**: 支持滚动查看所有设置
- **Slider/Toggle**: Unity标准控件

### OrdersPanel.prefab (订单面板)
**用途**: 显示和管理订单

**结构**:
```
OrdersPanel (GameObject)
├── Background (Image)
├── Title (TextMeshProUGUI)
├── OrdersContainer (ScrollRect)
│   └── Content (VerticalLayoutGroup)
│       ├── OrderItem1 (OrderItem prefab)
│       ├── OrderItem2 (OrderItem prefab)
│       └── ...
├── RefreshButton (Button)
└── CloseButton (Button)
```

### AchievementPanel.prefab (成就面板)
**用途**: 显示成就列表

**结构**:
```
AchievementPanel (GameObject)
├── Background (Image)
├── Title (TextMeshProUGUI)
├── ProgressText (TextMeshProUGUI)
├── AchievementsContainer (ScrollRect)
│   └── Content (VerticalLayoutGroup)
│       ├── AchievementItem1 (AchievementItem prefab)
│       ├── AchievementItem2 (AchievementItem prefab)
│       └── ...
└── CloseButton (Button)
```

---

## 创建步骤

### 步骤1: 创建基础Canvas
1. 在Scene中创建UI Canvas
2. 设置Canvas Scaler (1080x1920, ScaleWithScreenSize)
3. 添加EventSystem (如果不存在)

### 步骤2: 创建各个面板
1. 为每个UI组件创建空的GameObject
2. 添加必要的UI组件 (Image, TextMeshProUGUI, Button等)
3. 设置布局和样式
4. 添加对应的脚本组件
5. 拖拽到UIManager的对应字段中

### 步骤3: 创建预制件
1. 将配置好的UI对象拖拽到Prefabs/UI/文件夹中
2. 删除Scene中的临时对象
3. 测试预制件是否正确显示

### 步骤4: 资源导入
1. 在Resources/Icons/Items/中添加物品图标PNG文件
2. 在Resources/UI/中添加UI精灵资源
3. 在Resources/Audio/中添加音效文件

---

## 注意事项

1. **分辨率适配**: 使用Canvas Scaler确保不同屏幕适配
2. **字体**: 确保TextMeshPro使用正确的字体资源
3. **颜色主题**: 保持UI风格一致
4. **交互反馈**: 按钮要有悬停和点击状态
5. **性能**: 避免过多的UI元素同时显示

---

## 测试验证

创建完成后，通过以下方式测试：
1. 运行游戏，按U键查看UI信息
2. 按I/O/A键测试各个面板显示
3. 检查布局是否正确，文字是否清晰
4. 测试按钮交互和面板切换