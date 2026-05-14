# SaveWorld 微信小游戏架构文档

> 基于现有 Unity 项目 + 微信小游戏转换插件 (minigame-unity-webgl-transform)
> 文档版本: v1.0 | 日期: 2026-05-13

---

## 一、当前状态总览

### 已完成的工作

你的项目已经有完整的微信小游戏导出基础：

| 组件 | 状态 | 说明 |
|------|------|------|
| Unity WebGL 构建 | ✅ 已完成 | `Builds/WebGL/minigame/` 已生成 |
| 微信转换插件 | ✅ 已集成 | UnityPlugin v1.2.91, `minigame-unity-webgl-transform` |
| WASM 运行时 | ✅ 就绪 | `wasmcode/` 分包已生成 |
| 资源分包 | ✅ 就绪 | `data-package/` 分包已配置 |
| game.json 配置 | ✅ 基础完成 | 竖屏模式 + iOS高性能 + 分包 |
| project.config.json | ✅ 已配置 | AppID: `wxb2624b9ca163b59f` |
| 加载页配置 | ✅ 就绪 | 背景 + 进度条 + 文案 |
| Unity 版 C# 微信系统 | ✅ 框架完成 | 7 个子系统全部使用 `WeChatWASM` SDK |

### 微信 C# 系统清单

```
WeChatManager          → SDK 初始化 + 登录 + 分享 + 广告 + 支付
WeChatLoginSystem      → wx.login + 用户信息获取 + 授权按钮
WeChatShareSystem      → 分享好友 + 分享成就/进度/排行 + 分享菜单
WeChatAdSystem         → 激励视频 + Banner + 插屏广告
WeChatPaySystem        → 米大师支付 + 虚拟支付
WeChatSocialSystem     → 排行榜分数上传 + 开放数据域
WeChatStorageSystem    → 本地存储 + 云存储
WeChatConfig           → ScriptableObject 配置 (AppID/广告ID/支付offerId)
WeChatConfigManager    → 配置统一管理器
WeChatBuild            → Editor 构建自动化脚本
```

### 构建产物结构

```
Builds/WebGL/minigame/                    # ← 用微信开发者工具打开此目录
├── game.js                               # 入口，加载 UnityPlugin 插件
├── game.json                             # 小游戏配置
├── project.config.json                   # 项目配置 (AppID等)
├── weapp-adapter.js                      # 微信API适配层 (74KB)
├── unity-namespace.js                    # Unity运行时命名空间配置
├── webgl.wasm.framework.unityweb.js      # Unity WebGL Framework (~933KB)
├── events.js                             # 事件管理器
├── plugin-config.js                      # 启动配置
├── check-version.js                      # 版本兼容检查
├── texture-config.js                     # 纹理配置
├── images/                               # 加载页素材
│   ├── background.jpg
│   └── unity_logo.png
├── unity-sdk/                            # 微信SDK桥接层 (JS)
│   ├── index.js                          # SDK入口
│   ├── ad.js                             # 广告
│   ├── share.js                          # 分享
│   ├── cloud.js                          # 云开发
│   ├── storage.js                        # 存储
│   ├── open-data.js                      # 开放数据域
│   ├── authorize.js                      # 授权
│   ├── userinfo.js                       # 用户信息
│   ├── audio/                            # 音频
│   ├── touch/                            # 触控
│   ├── fs.js                             # 文件系统
│   └── ...
├── framework/                            # 框架代码
├── workers/                              # Web Worker
├── wasmcode/                             # WASM代码分包
│   └── 49aecbc9587171fa.webgl.wasm.code.unityweb.wasm.br
└── data-package/                         # 游戏资源分包
    └── 04976dbb12496579.webgl.data.unityweb.bin.txt
```

---

## 二、整体架构

```
┌─────────────────────────────────────────────────────────────────────┐
│                    微信小游戏运行时                                  │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  game.js (入口)                                            │   │
│  │  → requirePlugin('UnityPlugin') 加载转换插件               │   │
│  │  → UnityManager 管理启动流程                                │   │
│  │  → 加载 loadingPage → 加载WASM → 编译 → 加载资源 → 运行    │   │
│  └──────────────┬──────────────────────────────────────────────┘   │
│                 │                                                   │
│  ┌──────────────▼──────────────────────────────────────────────┐   │
│  │  unity-sdk/ (微信SDK桥接层)                                │   │
│  │  weapp-adapter.js → polyfill wx.* → 对齐 Web API          │   │
│  │  C# WeChatWASM.WX.* 调用 → 通过JS binding → wx.* API      │   │
│  └──────────────┬──────────────────────────────────────────────┘   │
│                 │                                                   │
│  ┌──────────────▼──────────────────────────────────────────────┐   │
│  │  WASM 运行时 (Unity Engine in WASM)                        │   │
│  │  三层洋葱架构:                                              │   │
│  │  ┌─────────────────────────────────────────────────────┐    │   │
│  │  │  行为层: CraftingEngine / ExplorationEngine /        │    │   │
│  │  │          OrderEngine / PlayerManager / GridManager   │    │   │
│  │  │          WeChatManager / AchievementSystem / ...     │    │   │
│  │  └──────────────────────┬──────────────────────────────┘    │   │
│  │  ┌──────────────────────▼──────────────────────────────┐    │   │
│  │  │  事件层: EventBus (发布/订阅)                       │    │   │
│  │  └──────────────────────┬──────────────────────────────┘    │   │
│  │  ┌──────────────────────▼──────────────────────────────┐    │   │
│  │  │  状态层: GameState (不可变) + StateMutator           │    │   │
│  │  └─────────────────────────────────────────────────────┘    │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────────┐   │
│  │  开放数据域      │  │  CDN 资源服务器  │  │  游戏后端服务器   │   │
│  │  (排行榜渲染)    │  │  (DATA_CDN)     │  │  (登录/存档/支付) │   │
│  └─────────────────┘  └─────────────────┘  └──────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 三、核心配置调优

### 3.1 game.json 完善建议

当前 `game.json` 是基础配置，以下是需要补充的关键项：

```json
{
  "deviceOrientation": "portrait",
  "iOSHighPerformance": true,
  "showStatusBarStyle": "light-content",
  "openDataContext": "open-data",
  "subpackages": [
    {
      "name": "wasmcode",
      "root": "wasmcode/"
    },
    {
      "name": "data-package",
      "root": "data-package/"
    }
  ],
  "parallelPreloadSubpackages": [
    { "name": "wasmcode" }
  ],
  "plugins": {
    "UnityPlugin": {
      "version": "1.2.91",
      "provider": "wxe5a48f1ed5f544b7",
      "contexts": [{ "type": "isolatedContext" }]
    }
  },
  "workers": "workers",
  "networkTimeout": {
    "request": 10000,
    "connectSocket": 10000,
    "uploadFile": 30000,
    "downloadFile": 60000
  }
}
```

**新增项说明：**

| 配置项 | 作用 |
|--------|------|
| `showStatusBarStyle` | 状态栏样式，与游戏UI融合 |
| `openDataContext` | 开放数据域路径（排行榜需要） |
| `networkTimeout` | 网络超时，防止CDN资源加载卡住启动 |

### 3.2 DATA_CDN 配置（关键）

当前 `game.js` 中 `DATA_CDN` 指向 `http://localhost:8080`，上线前**必须替换为真实CDN地址**：

```javascript
// game.js 中修改
DATA_CDN: 'https://your-cdn-domain.com',  // 替换为你的CDN
```

**资源加载流程：**
1. 首包（主包）：game.js + weapp-adapter + framework + loadingPage → 立即加载
2. `wasmcode` 分包：WASM 代码（并行预加载）
3. `data-package` 分包：游戏资源文件（从 CDN 下载，约 21MB）

### 3.3 加载页优化

当前加载页配置在 `game.js` 的 `loadingPageConfig` 中：

```javascript
loadingPageConfig: {
    totalLaunchTime: 7000,        // 预计总加载时间(ms)
    materialConfig: {
        backgroundImage: 'images/background.jpg',    // 替换为游戏风格背景
        iconImage: 'images/unity_logo.png',           // 替换为游戏Logo
    },
    textConfig: {
        firstStartText: '首次加载请耐心等待',     // 可改为更贴合游戏的文案
        downloadingText: ['正在加载资源'],
        compilingText: '编译中',
        initText: '初始化中',
        completeText: '开始游戏',
    },
}
```

**建议：**
- `images/background.jpg` 替换为 SaveWorld 末日主题的 750x1334 背景图
- `images/unity_logo.png` 替换为游戏 Logo
- `firstStartText` 改为 "末世将至，准备生存..."

### 3.4 unity-namespace.js 关键调优

```javascript
// 当前配置
unityNamespace.unityHeapReservedMemory: 256,     // 堆内存 256MB
unityNamespace.releaseMemorySize: 31457280,       // 30MB 释放阈值
unityNamespace.isDevelopmentBuild: true,          // ← 上线前必须改为 false
unityNamespace.enableDebugLog: false,             // 保持关闭
unityNamespace.hideTimeLogModal: true,            // 开发时隐藏耗时弹框
unityNamespace.iOSAutoGCInterval: 10000,          // iOS GC间隔 10秒
```

**上线前必须修改：**
| 参数 | 当前值 | 建议值 | 原因 |
|------|--------|--------|------|
| `isDevelopmentBuild` | `true` | `false` | Development Build 包含调试符号，包体大 |
| `enableDebugLog` | `false` | `false` | 保持关闭，减少性能开销 |
| `hideTimeLogModal` | `true` | `true` | 线上不需要耗时弹框 |

---

## 四、WeChatConfig 配置（Resources/WeChatConfig.asset）

在 Unity Editor 中创建 `Assets/Resources/WeChatConfig.asset`，填写以下信息：

### 4.1 必须配置项

| 字段 | 在哪里获取 | 示例值 |
|------|-----------|--------|
| `appId` | 微信公众平台 → 开发 → 开发管理 | `wxb2624b9ca163b59f` (已有) |
| `shareTitle` | 自定义 | "末世生存合成" |
| `shareImageUrl` | 上传到CDN后填入 | `https://cdn.example.com/share.jpg` |

### 4.2 广告配置（申请流量主后）

| 字段 | 在哪里获取 | 说明 |
|------|-----------|------|
| `rewardedVideoAdUnitId` | 微信MP后台 → 流量主 → 广告管理 → 新建广告位 | 激励视频（体力恢复/探索加倍） |
| `bannerAdUnitId` | 同上 | Banner广告 |
| `interstitialAdUnitId` | 同上 | 插屏广告（结算/返回主界面时） |

### 4.3 支付配置（接入米大师后）

| 字段 | 在哪里获取 | 说明 |
|------|-----------|------|
| `midasOfferId` | 微信MP后台 → 支付 → 米大师 | 商品ID |
| `midasZoneId` | 同上 | 分区ID，单区填 "1" |
| `midasEnv` | 0=正式 / 1=沙箱 | 先用沙箱测试 |

---

## 五、开放数据域（排行榜）

排行榜需要通过开放数据域实现（微信安全限制，好友数据只能在独立线程中访问）。

### 5.1 创建 open-data 目录

```
Builds/WebGL/minigame/
└── open-data/                    # ← 需要创建
    ├── index.js                  # 开放数据域入口
    ├── game.json                 # 开放数据域配置（空对象即可）
    └── components/               # 排行榜渲染组件（可选）
```

### 5.2 open-data/index.js

```javascript
// 开放数据域 - 排行榜渲染
const sharedCanvas = wx.getSharedCanvas();
const ctx = sharedCanvas.getContext('2d');

// 获取好友排行数据
wx.getFriendCloudStorage({
    keyList: ['game_score', 'game_level'],
    success: (res) => {
        const data = res.data;
        // data 是好友的 KVData 列表
        data.sort((a, b) => {
            const scoreA = a.KVDataList.find(kv => kv.key === 'game_score');
            const scoreB = b.KVDataList.find(kv => kv.key === 'game_score');
            return (parseInt(scoreB?.value || '0')) - (parseInt(scoreA?.value || '0'));
        });
        renderLeaderboard(data);
    },
    fail: (err) => {
        console.error('获取好友排行失败:', err);
    }
});

function renderLeaderboard(data) {
    ctx.clearRect(0, 0, sharedCanvas.width, sharedCanvas.height);

    // 背景
    ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
    ctx.fillRect(0, 0, sharedCanvas.width, sharedCanvas.height);

    // 标题
    ctx.fillStyle = '#ffffff';
    ctx.font = 'bold 24px sans-serif';
    ctx.textAlign = 'center';
    ctx.fillText('好友排行榜', sharedCanvas.width / 2, 50);

    // 列表（前20名）
    const startY = 90;
    const lineHeight = 50;
    ctx.font = '16px sans-serif';
    ctx.textAlign = 'left';

    data.slice(0, 20).forEach((item, index) => {
        const y = startY + index * lineHeight;

        // 排名
        ctx.fillStyle = index < 3 ? '#FFD700' : '#cccccc';
        ctx.fillText(`#${index + 1}`, 30, y);

        // 昵称
        ctx.fillStyle = '#ffffff';
        ctx.fillText(item.nickname || '未知玩家', 80, y);

        // 分数
        const scoreData = item.KVDataList.find(kv => kv.key === 'game_score');
        const levelData = item.KVDataList.find(kv => kv.key === 'game_level');
        ctx.fillText(`Lv.${levelData?.value || 1}  ${scoreData?.value || 0}分`,
                      250, y);
    });
}

// 监听主域消息（用于刷新等操作）
wx.onMessage((msg) => {
    if (msg.type === 'refresh') {
        // 重新获取排行数据
    }
    if (msg.type === 'show') {
        // 显示排行榜
    }
    if (msg.type === 'hide') {
        ctx.clearRect(0, 0, sharedCanvas.width, sharedCanvas.height);
    }
});
```

### 5.3 Unity 端集成排行榜

在 `WeChatSocialSystem.cs` 中已有的 `ShowOpenData` 方法即可渲染排行榜：

```csharp
// 使用示例（在 UI 层调用）
var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
WeChatSocialSystem.Instance.ShowOpenData(tex, x, y, width, height);

// 隐藏排行榜
WeChatSocialSystem.Instance.HideOpenData();
```

---

## 六、游戏内系统与微信功能的对接点

### 6.1 分享裂变（核心获客手段）

已有 `WeChatShareSystem`，需要在 Unity 游戏逻辑中找到合适的触发时机：

| 触发时机 | 调用方法 | 分享内容建议 |
|----------|----------|-------------|
| 合成出 L10 终极物品 | `ShareAchievement()` | "我在末世中合成了{物品名}！" |
| 等级达到里程碑 (10/20/50) | `ShareProgress()` | "我在末世中达到了{等级}级！" |
| 解锁全部格子 | `ShareProgress()` | "我解锁了全部63个格子！" |
| 右上角转发按钮 | `OnShareAppMessage()` | 被动分享，展示当前等级/最高合成 |
| 好友助力/邀请 | `InviteFriend()` | "来和我一起在末世中生存！" |

**在 GameEntry 或 UI 系统中接入：**

```csharp
// GameEntry.cs 初始化时
WeChatShareSystem.Instance.Initialize();

// 合成 L10 物品时触发分享
if (outputLevel == 10)
{
    WeChatShareSystem.Instance.ShareAchievement(
        ItemConfig.GetItemName(outputItem),
        "合成了终极物品"
    );
}

// 监听右上角分享按钮
WeChatShareSystem.Instance.UpdateShareMenu(
    $"我在《末世生存合成》中达到了{PlayerManager.Instance.GetLevel()}级",
    WeChatConfigManager.ShareImageUrl
);
```

### 6.2 激励广告（变现 + 留存）

已有 `WeChatAdSystem`，建议的接入点：

| 场景 | 广告类型 | 奖励 |
|------|----------|------|
| 体力耗尽想继续探索 | 激励视频 | +10 体力 |
| 探索结果翻倍 | 激励视频 | 本次探索产出 x2 |
| 订单奖励翻倍 | 激励视频 | 金币/经验 x2 |
| 回到背包时 | Banner | 底部常驻（可关闭） |
| 订单结算后 | 插屏 | 无奖励，纯变现 |

```csharp
// 探索前：体力不足 → 看广告恢复
if (PlayerManager.Instance.GetCurrentStamina() < 1)
{
    WeChatAdSystem.Instance.ShowRewardedVideoAd((success, _) =>
    {
        if (success)
        {
            PlayerManager.Instance.RecoverStamina(10);
            // 继续探索
        }
    });
}
```

### 6.3 订阅消息（召回）

```csharp
// 在 WeChatManager.cs 或新建 WeChatSubscribeSystem.cs 中
public void RequestSubscribeMessage()
{
    // 注意：需要模板ID，在MP后台 → 功能 → 订阅消息 中申请
    WX.RequestSubscribePayment(new RequestSubscribePaymentOption
    {
        // 使用 wx.requestSubscribeMessage 的 JS binding
    });
}
```

**推荐申请的模板：**
- 体力回满提醒
- 每日订单刷新
- 好友超越排行通知

### 6.4 分数上报（排行榜数据源）

```csharp
// 在 StateMutator 中，每次状态变更后更新云存储分数
int score = player.Level * 100 + achievementCount * 50;
WeChatSocialSystem.Instance.UpdateScore(score);

// 同时上报等级
WX.SetUserCloudStorage(new SetUserCloudStorageOption
{
    KVDataList = new KVData[]
    {
        new KVData { key = "game_score", value = score.ToString() },
        new KVData { key = "game_level", value = player.Level.ToString() }
    }
});
```

---

## 七、存档系统对接

### 7.1 策略：本地优先 + 云端备份

```
┌─────────────────────────────────────┐
│  StorageSystem (Unity 原有)         │
│  ├── SaveGameState() → JSON         │
│  └── LoadGameState() → GameState    │
└──────────────┬──────────────────────┘
               │ 现有 localStorage/WebGL
               ▼
┌─────────────────────────────────────┐
│  WeChatStorageSystem (新增层)       │
│  ├── SaveLocalSync() → wx.setStorage│  ← 替代 WebGL localStorage
│  ├── LoadLocalSync() → wx.getStorage│
│  └── SaveCloud() → wx.setUserCloudStorage │ ← 云端备份
└─────────────────────────────────────┘
```

**修改 StorageSystem 以适配微信环境：**

```csharp
// StorageSystem.cs 中，根据运行环境选择存储后端
#if UNITY_WEBGL && !UNITY_EDITOR
    // 微信小游戏环境：使用 WeChatStorageSystem
    public StorageResult SaveGameState(GameState gameState)
    {
        string json = JsonSerializer.Serialize(state);
        WeChatStorageSystem.Instance.SaveLocalSync("player_data", json);

        // 每5分钟云备份一次
        WeChatStorageSystem.Instance.SaveCloud("save_data", json);
        return StorageResult.Success;
    }
#else
    // 编辑器/其他平台：使用 PlayerPrefs
#endif
```

### 7.2 微信小游戏存储限制

| 限制 | 值 | 影响 |
|------|-----|------|
| 本地存储上限 | 默认 10MB | `maxStorage: 200` 已扩容到 200MB |
| 单个 key 上限 | 1MB | 存档 JSON 需要压缩 |
| 云存储 KV 个数 | 最多 10 个 KV | 只存关键数据（分数/等级/存档摘要） |
| 云存储单值上限 | 每个 KV 4KB | 存档需要拆分或只存哈希 |

---

## 八、性能优化

### 8.1 包体优化

| 优化项 | 当前 | 目标 | 方法 |
|--------|------|------|------|
| WASM 代码 | br压缩分包 | 保持 | 已是最佳实践 |
| 资源包 | ~21MB | <15MB | 压缩纹理 + 移除未使用资源 |
| 首包大小 | ~1MB | <4MB | 确保首包只含启动必需文件 |
| 加载时间 | 预计7秒 | <5秒 | CDN加速 + 资源分包 + preloadDataList |

### 8.2 运行时优化

| 优化项 | 方法 |
|--------|------|
| 内存控制 | `releaseMemorySize: 30MB`，及时释放不用的 AssetBundle |
| iOS高性能 | `iOSHighPerformance: true` 已开启，使用 Metal 渲染 |
| 定时GC | `iOSAutoGCInterval: 10000` (10秒)，防止内存峰值 |
| 帧率稳定 | 合成动画用 DOTween/LeanTween 而非 Update 循环 |
| setData 批量 | 如果有 Canvas UI 层，确保批量更新 |

### 8.3 CDN 配置

```
资源文件上传到 CDN：
  DATA_CDN/
  └── Assets/
      ├── StreamingAssets/
      │   ├── WebGL/
      │   │   ├── 04976dbb12496579.webgl.data.unityweb    # 游戏数据
      │   │   └── 04976dbb12496579.webgl.data.unityweb.br # br压缩版
      │   └── Textures/
      │       └── *.unityweb                                # 纹理文件
      └── ...
```

CDN 要求：
- 支持 HTTPS（必须）
- 支持 Range 请求（断点续传）
- 建议使用腾讯云 CDN（微信生态内速度最快）
- 开启 br/gzip 压缩

---

## 九、上线 Checklist

### 第一阶段：本地调试

- [ ] Unity Editor 中创建 `Assets/Resources/WeChatConfig.asset`
- [ ] 填入 AppID 和分享配置
- [ ] Unity 菜单 `WeChat → Build WebGL` 重新构建
- [ ] 安装微信开发者工具
- [ ] 打开 `Builds/WebGL/minigame/` 目录
- [ ] 确认游戏在开发者工具中正常启动和运行
- [ ] 测试合成/探索/订单核心玩法

### 第二阶段：微信功能对接

- [ ] 替换加载页背景和Logo (`images/`)
- [ ] 配置 `DATA_CDN` 为真实 CDN 地址
- [ ] 创建 `open-data/index.js` 排行榜
- [ ] 在 GameEntry 中初始化 `WeChatShareSystem`
- [ ] 在合成/探索/升级关键节点接入分享
- [ ] 接入激励广告（体力恢复场景）
- [ ] 修改 `WeChatStorageSystem` 作为存档后端
- [ ] 接入分数上报 `WeChatSocialSystem.UpdateScore()`

### 第三阶段：上线前优化

- [ ] `isDevelopmentBuild` 改为 `false`
- [ ] `enableDebugLog` 确认关闭
- [ ] 压缩纹理 + 移除未使用资源
- [ ] 上传资源到 CDN
- [ ] 资源分包 (`loadDataPackageFromSubpackage`) 测试
- [ ] iOS/Android 真机测试
- [ ] 内存泄漏检测（开发者工具 Performance 面板）
- [ ] 弱网环境测试

### 第四阶段：提交审核

- [ ] 微信MP后台完善游戏信息（名称/图标/简介/截图）
- [ ] 上传隐私保护指引
- [ ] 配置域名白名单（如有后端服务器）
- [ ] 测试账号提交
- [ ] 提交审核

---

## 十、后端需求（按优先级）

### MVP 阶段（可不需要后端）

纯本地运行，所有数据存 `wx.setStorage`。分享/广告等客户端直调微信API。

### V1.0 阶段（需要简单后端）

| 接口 | 用途 | 说明 |
|------|------|------|
| `POST /auth/login` | 登录 | wx.login code → 换取 openId + session_key |
| `POST /save/upload` | 存档上传 | 加密存档数据，防篡改 |
| `GET /save/download` | 存档下载 | 换设备时恢复进度 |
| `POST /analytics/event` | 数据上报 | 用户行为分析 |

### V1.1 阶段

| 接口 | 用途 | 说明 |
|------|------|------|
| `GET /rank/daily` | 每日排行 | 服务端排行（不受开放数据域限制） |
| `POST /friend/help` | 好友助力 | 社交裂变 |
| `GET /activity/list` | 活动列表 | 运营活动配置 |

**推荐后端方案：** 微信云开发（免域名备案，免服务器运维，与小游戏天然集成）

---

## 十一、获客策略（基于现有架构）

| 策略 | 实现基础 | 预期效果 |
|------|----------|----------|
| **分享裂变** | WeChatShareSystem 已完成 | 核心获客手段 |
| **好友排行攀比** | WeChatSocialSystem + open-data 已有框架 | 社交传播 |
| **激励广告+体力** | WeChatAdSystem 已完成 | 变现+留存 |
| **每日任务/签到** | 需在 Unity 中新增 | 提升日活 |
| **新手引导优化** | 需在 Unity 中新增 | 降低首日流失 |
| **限时活动** | 需在 Unity 中新增 | 提升回访 |

---

## 十二、现有 C# 代码无需修改的部分

以下 Unity C# 代码可以直接在微信小游戏 WASM 中运行，无需任何修改：

- ✅ `Core/` — GameState, StateMutator, EventBus, GameEntry
- ✅ `Grid/` — GridManager, GridCell
- ✅ `Crafting/` — CraftingEngine
- ✅ `Exploration/` — ExplorationEngine
- ✅ `Order/` — OrderEngine
- ✅ `Items/` — ItemConfig, ItemIconManager
- ✅ `Player/` — PlayerManager
- ✅ `Achievement/` — AchievementSystem
- ✅ `Audio/` — AudioManager
- ✅ `Analytics/` — AnalyticsSystem
- ✅ `UI/` — UIManager, BackpackUI 等 (Unity UI 直接在 Canvas 渲染)
- ✅ `WeChat/` — 所有 WeChat*System.cs (已使用 WeChatWASM SDK)

**核心优势：Unity 的全部游戏逻辑原封不动地跑在 WASM 中，C# 代码零修改。**
