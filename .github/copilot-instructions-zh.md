# TestWebGL Unity 项目 - AI 开发指南（中文版）

## 项目概览

**TestWebGL** 是一个 Unity 2022.3.46f1 WebGL 游戏项目，目标平台为**微信小游戏**。该项目结合了传统 Unity 游戏开发和微信 SDK 集成，用于移动端部署。

### 核心技术
- **引擎**: Unity 2022.3.46f1
- **平台**: WebGL（微信小游戏）
- **2D 开发栈**: Animation、Aseprite、PSD Importer、SpriteShape、Tilemap
- **构建系统**: Visual Studio 解决方案 + 微信小游戏工具
- **集成**: 微信 SDK（com.tencent.puerts.webgl、WX-WASM-SDK-V2）

## 项目结构

```
Assets/
├── Scenes/                      # 游戏场景（SampleScene.unity、Main.unity）
├── Resources/                   # 运行时加载资源（字体等）
├── WebGLTemplates/              # WebGL 构建模板（WXTemplate2022、WXTemplate2022TJ）
├── WX-WASM-SDK-V2/              # 微信 SDK 和配置
├── TextMesh Pro/                # TextMeshPro 资源
└── [Game Code Folders]/         # 自定义游戏脚本（根据需要添加）

ProjectSettings/
├── ProjectVersion.txt           # Unity 版本（2022.3.46f1）
├── ProjectSettings.asset        # 播放器和平台设置
├── EditorBuildSettings.asset    # 场景配置
├── GraphicsSettings.asset       # 渲染管线配置
└── [其他设置]                   # 质量、场景模板等

Builds/
└── WXGame/                      # 微信小游戏构建输出

Packages/
├── manifest.json                # 包依赖項目
└── lock.json                    # 锁定版本

[解决方案文件]
├── TestWebGL.sln                # 主 C# 解决方案
├── Assembly-CSharp.csproj       # 运行时脚本
└── Assembly-CSharp-Editor.csproj # 编辑器脚本
```

## 开发工作流

### 在 Unity 中打开项目
- 在 Visual Studio 中打开 `TestWebGL.sln` 或使用 Unity Hub
- Unity 版本：2022.3.46f1（兼容性要求）
- 打开后等待资源数据库刷新

### 构建为 WebGL
1. **编辑器构建**（用于测试）：
   - File → Build Settings → WebGL → Build
   - 输出：`Builds/WebGL/`（临时文件）

2. **微信小游戏构建**：
   - 使用 WX-WASM-SDK-V2 工具或自定义构建管道
   - 构建类型：WeChat 模板（2022 或 TJ 变体）
   - 输出：`Builds/WXGame/minigame/`
   - 配置：检查 `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`

### 场景管理
- **SampleScene.unity**: 主游戏场景（在 EditorBuildSettings 中配置）
- **Main.unity**: 备用或启动场景
- 提交前始终测试两个场景

### 常见开发任务

#### 添加游戏脚本
- 在 `Assets/Scripts/` 下创建文件夹（按功能分类：UI/、Gameplay/、Utils/ 等）
- 使用 C# 且扩展名为 `.cs`
- 通过 Inspector 或代码导入到场景
- 遵守微信 SDK 要求（无浏览器 API、无 DOM 操作等）

#### 资源管理
- **字体**: 放在 `Assets/Resources/Fonts/`（由 TextMeshPro 使用）
- **精灵/图形**: 使用 WebGL 兼容格式（PNG、JPG）
- **动画**: 支持 2D 动画框架（Aseprite/PSD 导入可用）

#### 微信 SDK 集成
- 所有微信 API 位于 `WX-WASM-SDK-V2/`
- 示例用法：参考 `Assets/WX-WASM-SDK-V2/Editor/` 文档
- 关键 API 入口：`Wx` 命名空间（见 `Wx.csproj`、`WxEditor.csproj`）
- 限制：无 DOM 访问、序列化差异、内存限制

## 代码规范

### 命名和结构
- **命名空间**: 使用 `MyProject.Feature` 模式（例如 `TestWebGL.Gameplay`、`TestWebGL.UI`）
- **类名**: PascalCase
- **方法名**: PascalCase
- **字段名**: camelCase（私有/受保护），序列化字段使用 `m_` 前缀
- **常量**: UPPER_SNAKE_CASE

### Unity 最佳实践
- 使用 `[SerializeField]` 让私有字段在 Inspector 中可见
- 使用 `[HideInInspector]` 隐藏不需要公开的字段
- 在 `Awake()` 或 `Start()` 中缓存 `GetComponent<T>()` 和 `Transform` 引用
- 使用 `OnDestroy()` 清理资源
- 尽量避免使用 `FindObjectOfType<T>()`，优先使用直接引用

### WebGL/微信 特定注意事项
- **无阻塞操作**: 避免紧密循环或同步文件 I/O
- **高效内存**: 保持更新循环简洁，避免在 `Update()` 中分配内存
- **异步操作**: 使用协程（`StartCoroutine()`）或 `async/await`
- **微信 API 调用**: 用 try-catch 包装，优雅处理网络故障
- **输入处理**: 使用 `Input.GetKey()` 或 `EventSystem`（WebGL 中避免使用 `OnMouseDown()`）

## 测试

### 手动测试
- 在编辑器中运行（Scene → Run）
- 在 WebGL 目标设备上构建并运行
- 使用微信应用或开发者工具测试

### 已知问题和解决方案
- WebGL 着色器限制：某些特殊效果可能需要简化
- 微信小游戏内存：目标构建大小 <50MB；优化资源
- CORS：本地测试时确保正确的网络服务器设置
- 平台特定行为：在真实微信设备上测试，不仅仅在桌面

## 构建命令和工具

### C# 构建（Visual Studio/Rider）
```
dotnet build TestWebGL.sln
dotnet build TestWebGL.sln -c Release
```

### 本地测试环境
- 使用 Python 简单 HTTP 服务器或其他方式提供 WebGL 服务
```
python -m http.server 8000  # 提供 builds/ 目录
```

### 微信工具
- 检查 `ProjectSettings/EditorBuildSettings.asset` 中的场景列表
- 使用微信开发者工具进行小游戏调试
- 查阅 `Builds/WXGame/minigame/project.config.json` 了解应用元数据

## 依赖项和包

**核心包**（来自 `Packages/manifest.json`）：
- `com.unity.feature.2d`: 完整的 2D 开发工具包
- `com.unity.test-framework`: 用于单元测试
- `com.unity.textmeshpro`: 高级文本渲染
- `com.unity.timeline`: 过场动画编辑（可选）
- `com.unity.visualscripting`: 可视化编程（可选）
- `com.qq.weixin.minigame`: 微信小游戏 SDK

**编辑器插件**：
- Rider、Visual Studio IDE 支持
- Collab Proxy、Plastic SCM

## 故障排除

### 项目无法打开
- 检查 Unity 版本是否为 2022.3.46f1（使用 Unity Hub）
- 删除 `Library/` 和 `Temp/` 文件夹，然后重新导入

### 构建失败
- 验证 `ProjectSettings/ProjectVersion.txt` 与本地 Unity 版本匹配
- 检查 `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset` 是否配置
- 确保 Visual Studio 中没有编译错误

### 微信集成问题
- 验证 `com.qq.weixin.minigame` 在 `Packages/manifest.json` 中
- 检查微信开发配置中的网络/CORS 设置
- 先在微信开发者工具中测试，再在真实设备上测试

### WebGL 性能优化
- 使用 Chrome DevTools（JavaScript Performance 标签）进行分析
- 减少绘制成本：批量精灵、使用图集
- 优化物理：禁用不必要的 2D 碰撞体
- 监控内存：微信中的堆内存目标 < 200MB

## AI 智能体业开发指南

### 本代码库的最佳实践
1. **修改前**：始终检查 EditorBuildSettings 中受影响的场景
2. **微信兼容性**：验证任何外部 API 用法是否与微信兼容
3. **跨平台意识**：代码运行在受限的 WebGL/小游戏环境，不是完整 PC
4. **资源导入**：考虑构建大小；优先选择简单格式（PNG 而非复杂 TIFF）
5. **版本稳定性**：未获得明确请求，不要升级到更新的 Unity 版本

### 推荐工作流
- **Bug 修复**: 首先检查微信 SDK 日志（网络、权限、内存）
- **新功能**: 在设计新游戏系统前验证微信 SDK 支持
- **重构**: 保持序列化字段结构，避免 Unity 场景数据丢失
- **测试**: 始终在 WebGL 预览 + 微信开发者工具中测试，不仅仅在编辑器中

### 应避免的反模式
- ❌ 在外部编辑器中直接修改 `.unity` 场景文件
- ❌ 使用已弃用的 `OnGUI()` 进行 UI 开发（使用 TextMeshPro + uGUI 代替）
- ❌ 在 StandaloneWindows 构建外调用特定操作系统 API（文件系统、注册表等）
- ❌ 同步网络调用（`WWW.LoadFromCacheOrDownload()` 阻塞）
- ❌ 在 `Update()` 或 `LateUpdate()` 中分配内存，不使用对象池

## 资源

- [Unity 2022.3 LTS 文档](https://docs.unity3d.com/2022.3/Documentation/)
- [WebGL 构建支持](https://docs.unity3d.com/Manual/webgl.html)
- [2D 功能概览](https://docs.unity3d.com/Manual/2D.html)
- 微信小游戏 SDK：检查 `Assets/WX-WASM-SDK-V2/` 中的本地文档
- [TextMeshPro 设置](https://docs.unity3d.com/Manual/TextMeshPro.html)

---

**最后更新**: 2026-03-11  
**维护人**: 开发团队

---

## 中文版说明

这是英文版 `copilot-instructions.md` 的完整中文翻译。

**用途**: 为中文开发团队提供项目开发指南和规范  
**保存位置**: `.github/copilot-instructions-zh.md`  
**维护**: 与英文版本保持同步

