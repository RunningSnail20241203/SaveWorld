# TestWebGL Unity Project - AI Development Guidelines

## Project Overview

**TestWebGL** is a Unity 2022.3.46f1 WebGL game project targeting **WeChat mini-games**. The project combines traditional Unity gameplay with WeChat SDK integration for mobile deployment.

### Key Technologies
- **Engine**: Unity 2022.3.46f1
- **Platform**: WebGL (WeChat mini-games)
- **2D Stack**: Animation, Aseprite, PSD Importer, SpriteShape, Tilemap
- **Build System**: Visual Studio solution + Weixin mini-game tools
- **Integration**: WeChat SDK (com.tencent.puerts.webgl, WX-WASM-SDK-V2)

## Project Structure

```
Assets/
├── Scenes/                      # Game scenes (SampleScene.unity, Main.unity)
├── Resources/                   # Runtime-loaded assets (Fonts, etc.)
├── WebGLTemplates/              # WebGL build templates (WXTemplate2022, WXTemplate2022TJ)
├── WX-WASM-SDK-V2/              # WeChat SDK and configuration
├── TextMesh Pro/                # TextMeshPro resources
└── [Game Code Folders]/         # Custom game scripts (add as needed)

ProjectSettings/
├── ProjectVersion.txt           # Unity version (2022.3.46f1)
├── ProjectSettings.asset        # Player and platform settings
├── EditorBuildSettings.asset    # Scene configuration
├── GraphicsSettings.asset       # Rendering pipeline
└── [Other Settings]             # Quality, scene templates, etc.

Builds/
└── WXGame/                      # WeChat mini-game build output

Packages/
├── manifest.json                # Package dependencies
└── lock.json                    # Locked versions

[Solution Files]
├── TestWebGL.sln                # Main C# solution
├── Assembly-CSharp.csproj       # Runtime scripts
└── Assembly-CSharp-Editor.csproj # Editor scripts
```

## Development Workflow

### Opening the Project in Unity
- Open `TestWebGL.sln` in Visual Studio or use Unity Hub
- Unity version: 2022.3.46f1 (required for compatibility)
- Wait for asset database refresh after opening

### Building for WebGL
1. **Editor Build** (for testing):
   - File → Build Settings → WebGL → Build
   - Output: `Builds/WebGL/` (temporary)

2. **WeChat Mini-Game Build**:
   - Use WX-WASM-SDK-V2 tools or custom build pipeline
   - Build type: WeChat Template (2022 or TJ variant)
   - Output: `Builds/WXGame/minigame/`
   - Configuration: Check `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`

### Scene Management
- **SampleScene.unity**: Main gameplay scene (configured in EditorBuildSettings)
- **Main.unity**: Alternative or startup scene
- Always test both scenes before commits

### Common Development Tasks

#### Adding Game Scripts
- Create folders under `Assets/Scripts/` (following domain: UI/, Gameplay/, Utils/, etc.)
- Use C# with `.cs` extension
- Import in scenes via Inspector or programmatically
- Comply with WeChat SDK requirements (no browser APIs, DOM manipulation, etc.)

#### Asset Management
- **Fonts**: Place in `Assets/Resources/Fonts/` (used by TextMeshPro)
- **Sprites/Graphics**: Use WebGL-compatible formats (PNG, JPG)
- **Animations**: Support 2D animation framework (Aseprite/PSD import available)

#### WeChat SDK Integration
- All WeChat APIs in `WX-WASM-SDK-V2/`
- Example usage: See `Assets/WX-WASM-SDK-V2/Editor/` documentation
- Key API entry: `Wx` namespace (see `Wx.csproj`, `WxEditor.csproj`)
- Constraints: No DOM access, serialization differences, memory limits

## Code Conventions

### Naming & Structure
- **Namespaces**: Use `MyProject.Feature` pattern (e.g., `TestWebGL.Gameplay`, `TestWebGL.UI`)
- **Classes**: PascalCase
- **Methods**: PascalCase
- **Fields**: camelCase (private/protected), `m_` prefix for serialized fields
- **Constants**: UPPER_SNAKE_CASE

### Unity Guidelines
- Use `[SerializeField]` for inspector-visible private fields
- Use `[HideInInspector]` for public fields you don't want exposed
- Cache `GetComponent<T>()`, `Transform` references in `Awake()` or `Start()`
- Use `OnDestroy()` to clean up resources
- Prefer `FindObjectOfType<T>()` sparingly; use direct references when possible

### WebGL/WeChat Specific
- **No blocking operations**: Avoid tight loops or synchronous file I/O
- **Memory efficient**: Keep update loops clean, avoid allocating in `Update()`
- **Async operations**: Use coroutines (`StartCoroutine()`) or `async`/`await`
- **WeChat API calls**: Wrap in try-catch; handle network failures gracefully
- **Input handling**: Use `Input.GetKey()` or `EventSystem` (not `OnMouseDown()` in WebGL)

## Testing

### Manual Testing
- Play mode in Editor (Scene → Run)
- Build and run in WebGL target device
- Test with WeChat app or developer tools

### Known Issues & Workarounds
- WebGL shader limitations: Some fancy effects may need simplification
- WeChat mini-game memory: Aim for builds < 50MB; optimize assets
- CORS: When testing locally, ensure proper web server setup
- Platform-specific behavior: Test on actual WeChat device, not just desktop

## Build Commands & Tools

### C# Build (Visual Studio/Rider)
```
dotnet build TestWebGL.sln
dotnet build TestWebGL.sln -c Release
```

### Local Testing Environment
- Use Python simple HTTP server or similar for WebGL serving
```
python -m http.server 8000  # Serve builds/
```

### WeChat Tools
- Check `ProjectSettings/EditorBuildSettings.asset` for scene list
- Use WeChat developer tools for mini-game debugging
- Consult `Builds/WXGame/minigame/project.config.json` for app metadata

## Dependencies & Packages

**Core Packages** (from `Packages/manifest.json`):
- `com.unity.feature.2d`: Complete 2D development kit
- `com.unity.test-framework`: For unit tests
- `com.unity.textmeshpro`: Advanced text rendering
- `com.unity.timeline`: Cinematic sequencing (optional)
- `com.unity.visualscripting`: Visual programming (optional)
- `com.qq.weixin.minigame`: WeChat mini-game SDK

**EditorPlugins**:
- Rider, Visual Studio IDE support
- Collab Proxy, Plastic SCM

## Troubleshooting

### Project Won't Open
- Check Unity version is 2022.3.46f1 (use Unity Hub)
- Delete `Library/` and `Temp/` folders, then reimport

### Build Failures
- Verify `ProjectSettings/ProjectVersion.txt` matches local Unity version
- Check `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset` is configured
- Ensure no compiler errors in Visual Studio before building

### WeChat Integration Issues
- Verify `com.qq.weixin.minigame` is in `Packages/manifest.json`
- Check network/CORS settings in WeChat dev configuration
- Test in WeChat dev tools first, then on real device

### Performance in WebGL
- Profile with Chrome DevTools (JavaScript Performance tab)
- Reduce draw calls: Batch sprites, use atlases
- Optimize physics: Disable unnecessary 2D colliders
- Monitor memory: Aim for < 200MB heap in WeChat

## AI Agent Guidance

### Best Practices for This Codebase
1. **Before making changes**: Always check EditorBuildSettings for affected scenes
2. **WeChat compatibility**: Verify any external API usage is WeChat-compatible
3. **Cross-platform awareness**: Code runs in constrained WebGL/mini-game environment, not full PC
4. **Asset imports**: Be mindful of build size; prefer simple formats (PNG, not complex TIFF)
5. **Version stability**: Don't upgrade to newer Unity versions without explicit request

### Preferred Workflows
- **Bug fixes**: Check WeChat SDK logs first (network, permissions, memory)
- **New features**: Verify WeChat SDK support before designing new gameplay systems
- **Refactoring**: Maintain serialized field structure to avoid Unity scene data loss
- **Testing**: Always test in WebGL preview + WeChat dev tools, not just Editor

### Anti-Patterns (Avoid)
- ❌ Directly modifying scene `.unity` files in external editors
- ❌ Using deprecated `OnGUI()` for UI (use TextMeshPro + uGUI instead)
- ❌ Calling OS-specific APIs (file system, registry, etc.) outside StandaloneWindows builds
- ❌ Synchronous network calls (`WWW.LoadFromCacheOrDownload()` blocking)
- ❌ Allocating memory in `Update()` or `LateUpdate()` without pooling

## Resources

- [Unity 2022.3 LTS Documentation](https://docs.unity3d.com/2022.3/Documentation/)
- [WebGL Build Support](https://docs.unity3d.com/Manual/webgl.html)
- [2D Features Overview](https://docs.unity3d.com/Manual/2D.html)
- WeChat Mini-Game SDK: Check `Assets/WX-WASM-SDK-V2/` for local docs
- [TextMeshPro Setup](https://docs.unity3d.com/Manual/TextMeshPro.html)

---

**Last Updated**: 2026-03-11  
**Maintained by**: Development Team
