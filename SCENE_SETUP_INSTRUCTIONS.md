# 场景设置说明

## 概述
为了确保游戏可以直接点击Play按钮运行，需要正确配置场景。我已经创建了`SceneSetup.cs`脚本来自动配置场景。

## 快速设置方法

### 方法1：使用SceneSetup脚本（推荐）

1. **打开Unity**并加载项目
2. **打开Main.unity场景**（在Assets/Scenes/目录下）
3. **在场景中创建一个空GameObject**：
   - 右键点击Hierarchy面板
   - 选择"Create Empty"
   - 重命名为"SceneSetup"
4. **挂载SceneSetup脚本**：
   - 选中SceneSetup GameObject
   - 在Inspector面板中点击"Add Component"
   - 搜索并添加"SceneSetup"脚本
5. **配置UI预制件**（可选）：
   - 如果有UI预制件，可以拖拽到对应的字段
   - 如果没有预制件，系统会自动创建动态UI
6. **点击Play按钮**运行游戏

### 方法2：手动设置

1. **创建GameManager**：
   - 创建空GameObject命名为"GameManager"
   - 挂载GameManager脚本

2. **创建UIManager**：
   - 创建空GameObject命名为"UIManager"
   - 挂载UIManager脚本

3. **创建GameEntry**：
   - 创建空GameObject命名为"GameEntry"
   - 挂载GameEntry脚本

4. **配置EventSystem**：
   - 创建空GameObject命名为"EventSystem"
   - 添加EventSystem组件
   - 添加StandaloneInputModule组件

5. **配置相机**：
   - 设置背景颜色为深蓝灰色 (0.2, 0.3, 0.4)
   - 设置视野为60度

## 游戏运行说明

### 调试按键
游戏运行后，可以使用以下按键进行测试：

- **G键**：打印格子信息
- **P键**：打印玩家信息
- **C键**：打印合成信息
- **E键**：获得50经验
- **S键**：消耗1体力
- **X键**：执行探索
- **1-4键**：各阶段测试
- **U键**：显示UI信息
- **I键**：显示设置面板
- **O键**：显示订单面板
- **A键**：显示成就面板

### 游戏功能
游戏包含以下核心功能：

1. **格子系统**：63格背包，9行×7列
2. **合成系统**：双击合成、拖拽解锁
3. **探索系统**：消耗体力获得物品
4. **订单系统**：每日自动生成5个订单
5. **存储系统**：本地存档
6. **UI系统**：完整的用户界面

## 故障排除

### 如果游戏无法运行

1. **检查控制台错误**：
   - 打开Window > General > Console
   - 查看是否有编译错误

2. **检查场景配置**：
   - 确保Main.unity场景存在
   - 确保场景中有必要的GameObject

3. **检查脚本引用**：
   - 确保所有脚本都正确挂载
   - 检查是否有缺失的引用

4. **重新导入资源**：
   - 右键点击Assets文件夹
   - 选择"Reimport All"

### 如果UI不显示

1. **检查Canvas**：
   - 确保场景中有Canvas组件
   - 检查Canvas的Render Mode设置

2. **检查EventSystem**：
   - 确保场景中有EventSystem
   - 检查StandaloneInputModule是否正确配置

3. **检查相机**：
   - 确保相机设置正确
   - 检查背景颜色和视野

## 性能优化

### 建议设置

1. **图形设置**：
   - 使用中等质量设置
   - 启用动态批处理

2. **内存设置**：
   - 设置合适的纹理压缩
   - 启用资源卸载

3. **WebGL设置**：
   - 启用压缩
   - 设置合适的内存限制

## 联系支持

如果遇到问题，请检查：
1. Unity控制台错误信息
2. 文件路径是否正确
3. 所有脚本是否正确编译

游戏应该可以直接运行，无需额外配置！