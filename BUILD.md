# 微信小游戏构建与部署指南

> 最后更新: 2026-05-15

---

## 前置条件

1. ✅ 团结引擎 (Tuanjie) 已安装，版本 1.8.5（基于 Unity 2022.3.62t7）
2. ✅ 微信开发者工具已安装 ([下载](https://developers.weixin.qq.com/minigame/dev/devtools/download.html))
3. ✅ 微信小游戏 AppID: `wxb2624b9ca163b59f`
4. ✅ 项目已安装微信相关 SDK 包（`cn.tuanjie.minihost`、`com.qq.weixin.minigame`、`com.unity.instantgame`）

> **平台说明**：本项目使用**团结引擎 MiniGame 平台**构建，构建目标为 `BuildTarget.WeixinMiniGame`（而非标准 WebGL）。
> 微信 SDK 会自动将构建产物转换为微信小游戏格式，输出到 `minigame/` 子目录。

---

## 一、在 Unity 中配置 WeChatConfig

1. 打开 Unity Editor，加载 SaveWorld 项目
2. 在 Project 窗口右键 → `Create` → `WeChat` → `Config`
3. 命名为 `WeChatConfig`，放入 `Assets/Resources/` 目录
4. 填入微信后台获取的广告ID、支付offerId等（见下方配置说明）

> 如果不创建 .asset 文件，系统会使用内置默认值运行（广告和支付功能不可用）

---

## 二、微信后台配置（需要去 MP 平台操作）

### 广告配置
1. 登录 [微信公众平台](https://mp.weixin.qq.com/)
2. 左侧菜单 → `流量主` → `广告管理`
3. 新建广告位：
   - 激励视频广告 → 复制广告单元 ID → 填入 WeChatConfig
   - Banner 广告（可选）
   - 插屏广告（可选）

### 支付配置
1. 左侧菜单 → `支付` → `米大师`
2. 开通米大师支付
3. 获取 offerId → 填入 WeChatConfig
4. 配置支付回调地址（需要后端服务）

### 分享图片
1. 准备一张 500x400 的分享图
2. 上传到 CDN（如微信云开发存储）
3. 将 CDN 地址填入 WeChatConfig.shareImageUrl

---

## 三、Unity WebGL 构建

### Player Settings 关键配置

| 设置项 | 值 | 说明 |
|--------|-----|------|
| 构建平台 | MiniGame (WeixinMiniGame) | 团结引擎专用微信小游戏平台 |
| Color Space | Gamma | 微信小游戏要求 |
| Strip Engine Code | ✅ | 减小包体 |
| Managed Stripping Level | Medium (WeixinMiniGame) | 平衡大小和兼容性 |
| Code Optimization | Size | 优先包体大小 |
| Enable Exceptions | ExplicitlyThrown | MiniGame 平台调试用（发布时改为 None） |
| Compression Format | Brotli | 减小传输体积 |
| WebGL Template | WXTemplate2022TJ | 微信 SDK 自动使用团结版模板 |
| Memory Size | 256 MB | 初始 32MB，最大 2048MB |

> 以上配置由 `WeChatBuild.cs` 的 `ConfigurePlayerSettings()` 自动设置，也可在 Unity Editor → Player Settings → WeixinMiniGame 中手动调整。

### 禁用不用的内置模块（减小包体）

在 Player Settings → Other Settings → Strip Engine Code 开启后，以下模块会被自动剔除：
- Physics / Physics2D（本游戏只有 2D UI 交互）
- Terrain / TerrainPhysics
- Cloth
- VR

### 构建命令

```bash
# 方法1：Unity 命令行构建（推荐，使用自定义构建脚本）
# 注意：构建目标已从 WebGL 切换为 WeixinMiniGame
"<Tuanjie Editor 路径>/Editor/Tuanjie.exe" \
  -quit -batchmode \
  -projectPath "D:\Test\SaveWorld" \
  -executeMethod SaveWorld.Editor.WeChatBuild.Build \
  -logFile build.log

# 方法2：在 Unity Editor 中手动操作
# File → Build Settings → Platform: MiniGame (WeixinMiniGame) → Build
# 输出目录: Builds/WebGL/（微信 SDK 自动在此生成 minigame/ 子目录）

# 方法3：使用微信 SDK 内置构建（自动切换平台并构建）
# Unity Editor 菜单 → 微信小游戏 → 构建
```

> **关于输出目录**：虽然输出路径仍为 `Builds/WebGL/`，但微信 SDK 会自动在其下生成 `minigame/` 子目录，包含 `game.json`、`game.js` 等微信小游戏所需的全部文件。

---

## 四、首包资源部署到 CDN

> **重要**：微信小游戏所有分包合计不超过 **20MB**，本项目首包资源（data + wasm）约 28MB，超出限制，**必须使用 CDN 加载**。

### 4.1 构建后切换为 CDN 模式

1. 打开微信开发者工具，导入 `Builds/WebGL/minigame/`
2. 详情 → 本地设置 → 找到「首包资源加载方式」下拉框
3. 将「小游戏包内」改为 **CDN**

### 4.2 上传资源到 CDN

将 `Builds/WebGL/webgl/` 目录完整上传到 CDN，保持目录结构不变：

```
CDN 根目录/
├── 03cf87d209b02f45.webgl.wasm.code.unityweb.wasm.br
├── be840ea7ac8fda11.webgl.data.unityweb.bin.txt
├── Build/
├── StreamingAssets/
├── TemplateData/
├── boot.config
└── index.html
```

### 4.3 配置 DATA_CDN

构建产物 `minigame/game.js` 中的 `DATA_CDN` 字段需要指向 CDN 地址：

```js
// game.js（构建后由 SDK 自动生成）
const managerConfig = {
    DATA_CDN: 'https://your-cdn.com/saveworld/',  // ← 改为你的 CDN 地址
    // ...
};
```

> **注意**：每次重新构建后，`game.js` 会被覆盖，需要重新修改 `DATA_CDN`。
> 可通过构建后脚本自动替换，或使用下述本地 CDN 方案调试。

### 4.4 本地 CDN 调试

开发阶段可使用项目自带的本地 CDN 服务器：

```bash
# 项目根目录下执行
node start-local-cdn.js 18765

# 或在 Unity Editor 菜单：WeChat → Local CDN → Start
```

启动后将 `game.js` 中 `DATA_CDN` 设为 `http://localhost:18765`，即可在微信开发者工具中调试。

本地 CDN 特性：
- 自动 gzip / brotli 压缩
- 支持 Range 断点续传
- CORS 跨域已配置

### 4.5 CDN 加载流程说明

| 模式 | data 文件 (19.6MB) | wasm 代码 (8.7MB) | 首包总大小 |
|------|---------------------|---------------------|------------|
| 小游戏包内 | 打包进分包 | 打包进分包 | ~29MB（超限） |
| **CDN** | **远程下载** | **远程下载** | **~2-4MB** |

切换到 CDN 模式后，资源加载流程：
1. 用户打开小游戏 → 加载 minigame/ 代码包（~2-4MB）
2. 小游戏启动 → 通过 `DATA_CDN` 地址从 CDN 下载 data 和 wasm
3. 下载完成 → 游戏初始化完成，进入主界面

---

## 五、微信开发者工具调试

1. 打开微信开发者工具
2. 导入项目 → 选择 `Builds/WebGL/minigame/` 目录
3. AppID 选择 `wxb2624b9ca163b59f`
4. 点击「编译」→ 在模拟器中预览
5. 点击「预览」→ 手机扫码真机测试

---

## 六、上传与发布

1. 微信开发者工具 → 右上角「上传」
2. 版本号格式: `v1.0.0`，填写更新说明
3. 登录 [微信公众平台](https://mp.weixin.qq.com/) → 版本管理
4. 选择已上传版本 → 「提交审核」
5. 审核通过后 → 「发布」

---

## 七、集成测试清单

### 基础功能
- [ ] 微信 SDK 初始化成功（控制台无报错）
- [ ] wx.login 获取 code 成功
- [ ] 获取用户信息（头像、昵称）成功
- [ ] 游戏正常启动，进入主界面

### 核心玩法
- [ ] 63 格背包正常显示
- [ ] 双击合成正常
- [ ] 拖拽解锁正常
- [ ] 探索系统正常（消耗体力，产出物品）
- [ ] 订单系统正常（提交、奖励发放）

### 存储
- [ ] 游戏数据保存到 WX.Storage 成功
- [ ] 退出重进后数据恢复正确
- [ ] 云存储同步正常

### 分享
- [ ] 被动分享（右上角转发）正常
- [ ] 主动分享（按钮触发）正常
- [ ] 分享后回到游戏状态正常

### 广告
- [ ] 激励视频广告加载成功
- [ ] 激励视频完整观看后发放奖励
- [ ] Banner 广告显示正常
- [ ] 插屏广告显示正常

### 支付（沙箱环境）
- [ ] 米大师支付流程正常
- [ ] 支付回调处理正确
- [ ] 虚拟支付正常

### 离线兜底
- [ ] 非微信环境自动进入离线模式
- [ ] 离线模式下核心玩法正常
- [ ] 离线模式下 PlayerPrefs 存储正常

### 性能
- [ ] 首屏加载时间 < 3 秒
- [ ] 主包（minigame/）大小 < 4MB
- [ ] CDN 资源下载正常，无 404
- [ ] 帧率稳定 60fps（微信开发者工具模拟器）
- [ ] 内存占用稳定，无泄漏

### 兼容性
- [ ] iOS 微信客户端测试通过
- [ ] Android 微信客户端测试通过
- [ ] 不同屏幕尺寸适配正常

---

## 八、常见问题

### Q: 构建后微信开发者工具报 "game.json not found"
A: 确保 Unity WebGL Build 输出到 `Builds/WebGL/`，且该目录下存在 `minigame/game.json`

### Q: 包体超过 4MB 限制
A:
1. 确保已切换为 CDN 模式（详见第四章）
2. 首包资源（data、wasm）应通过 CDN 加载，不打包进小程序
3. 使用分包加载
4. 开启 Strip Engine Code
5. 纹理压缩为 ASTC/ETC2
6. 音频压缩

### Q: 切换 CDN 模式后资源加载失败（404）
A:
1. 确认 `game.js` 中 `DATA_CDN` 地址正确（末尾需带 `/`）
2. 确认 CDN 上已上传完整的 `webgl/` 目录内容
3. 确认 CDN 域名已在微信公众平台「开发管理 → 开发设置 → 服务器域名」中添加为 downloadFile 合法域名
4. 本地调试时使用 `node start-local-cdn.js` 并将 `DATA_CDN` 设为 `http://localhost:18765`

### Q: 每次构建后 DATA_CDN 被重置
A: `game.js` 由构建流程自动生成，每次构建会覆盖。可在 `WeChatBuild.cs` 中添加构建后自动替换脚本，或手动修改。

### Q: 微信登录返回 code 但需要 openId
A: 需要在后端服务器用 code + appSecret 换取 openId。开发阶段可先使用 code 作为标识。

### Q: 广告加载失败
A: 
1. 确认 appid 正确
2. 确认广告单元 ID 已创建并通过审核
3. 微信开发者工具中广告功能受限，需要真机测试
