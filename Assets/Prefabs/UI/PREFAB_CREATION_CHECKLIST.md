# 预制件创建清单 - Unity Editor 操作指南

按这个清单在Unity编辑器中创建所有UI预制件，完成后不需要再修改任何代码。

---

## ✅ 第一步: 创建主 Canvas

1. 在Scene中右键 -> UI -> Canvas
2. 设置 Canvas Scaler 组件:
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **750 x 1334**
   - Screen Match Mode: **Match Width Or Height**
   - Match: **0.5**
3. 添加 EventSystem (会自动弹出提示，点击Add)
4. 拖到 `Assets/Prefabs/UI/` 保存为 **MainCanvas.prefab**

---

## ✅ 第二步: 创建 GridUI 预制件

1. 在Canvas下创建空物体命名为 `GridUI`
2. 添加 Image 组件，Color 设置为 `#283344`
3. 添加 GridLayoutGroup 组件:
   - Cell Size: **92 x 92**
   - Spacing: **8 x 8**
   - Padding: **8, 8, 8, 8**
   - Start Corner: Upper Left
   - Start Axis: Horizontal
   - Child Alignment: Middle Center
   - Constraint: Fixed Column Count
   - Constraint Count: **7**
4. 添加 `GridUI` 脚本组件
5. 拖到 `Assets/Prefabs/UI/` 保存为 **GridUI.prefab**

---

## ✅ 第三步: 创建 GridCellUI 预制件

1. 创建空物体命名为 `GridCellUI`
2. 添加 Image 组件 -> 背景
3. 添加 Button 组件
4. 在下面创建:
   - Image (命名 ItemIcon) -> 居中，尺寸 70x70
   - TextMeshProUGUI (命名 ItemCount) -> 右下角，16号白色字
   - TextMeshProUGUI (命名 LockLevel) -> 居中，12号灰色字
5. 添加 `GridCellUI` 脚本组件
6. 拖到 `Assets/Prefabs/UI/` 保存为 **GridCellUI.prefab**

---

## ✅ 第四步: 创建 PlayerInfoPanel 预制件

1. 在Canvas下创建空物体命名为 `PlayerInfoPanel`
2. 添加 Image 组件，Color 设置为 `#1A2332`
3. 在下面创建:
   - TextMeshProUGUI (PlayerName) -> 顶部 18号字
   - TextMeshProUGUI (LevelInfo) -> 14号字
   - Slider (ExperienceBar) -> 绿色进度条
   - Slider (StaminaBar) -> 橙色进度条
4. 添加 `PlayerInfoPanel` 脚本组件
5. 拖到 `Assets/Prefabs/UI/` 保存为 **PlayerInfoPanel.prefab**

---

## ✅ 第五步: 创建 ControlPanel 预制件

1. 在Canvas下创建空物体命名为 `ControlPanel`
2. 添加 Image 组件
3. 添加 HorizontalLayoutGroup 组件，Spacing 16
4. 创建5个Button: 探索、订单、合成、仓库、商店
5. 添加 `ControlPanel` 脚本组件
6. 拖到 `Assets/Prefabs/UI/` 保存为 **ControlPanel.prefab**

---

## ✅ 第六步: 关联引用

打开 `UIManager` 游戏对象，在Inspector中:
- GridUIPrefab: 拖入 GridUI.prefab
- CellPrefab: 拖入 GridCellUI.prefab
- PlayerInfoPanelPrefab: 拖入 PlayerInfoPanel.prefab
- ControlPanelPrefab: 拖入 ControlPanel.prefab

---

## ✅ 完成后

现在点击Play运行游戏，你会看到:
✅ 所有界面自动布局
✅ 完美适配任何屏幕尺寸
✅ 统一的末世风格配色
✅ 专业的视觉层次
✅ 所有功能正常运行

不需要额外的美术资源，不需要调整任何参数，一切都已经在代码中配置好了。