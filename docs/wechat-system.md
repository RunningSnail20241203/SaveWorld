# 微信系统 (WeChat System)

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/WeChat/`

---

## 概述

微信小游戏 API 的统一接入层。所有微信相关功能通过事件总线与游戏核心通信。

---

## WeChatManager

**文件**: `WeChat/WeChatManager.cs` | **命名空间**: `SaveWorld.Game.WeChat`

### 职责
微信 API 统一入口，封装登录、分享、广告、支付等功能。

### 核心方法

```csharp
// 微信登录
public void Login()

// 分享游戏
public void Share(ShareType type, string title, string imageUrl)

// 播放激励广告
public void ShowRewardedAd(string adUnitId)

// 发起支付
public void RequestPayment(string orderId, long amount)
```

### ShareType 枚举

```csharp
public enum ShareType
{
    Friends = 1,    // 好友
    Moments = 2,    // 朋友圈
    Favorite = 3    // 收藏
}
```

---

## 子系统列表

| 子系统 | 文件 | 职责 |
|--------|------|------|
| 微信API | `WeChatAPI.cs` | 底层API封装 |
| 登录系统 | `WeChatLoginSystem.cs` | 登录流程管理 |
| 广告系统 | `WeChatAdSystem.cs` | 激励/插屏广告 |
| 分享系统 | `WeChatShareSystem.cs` | 分享配置与回调 |
| 支付系统 | `WeChatPaySystem.cs` | 微信支付流程 |
| 社交系统 | `WeChatSocialSystem.cs` | 好友/排行榜 |
| 存储系统 | `WeChatStorageSystem.cs` | 微信云存储 |

---

## 微信事件定义

```csharp
public class WeChatInitializedEvent : GameEvent { public bool Success; }
public class WeChatLoginStartedEvent : GameEvent { }
public class WeChatLoginCompletedEvent : GameEvent { public string OpenId; public bool Success; }
public class WeChatShareStartedEvent : GameEvent { public ShareType ShareType; }
public class WeChatShareCompletedEvent : GameEvent { public ShareType ShareType; public bool Success; }
public class RewardedAdStartedEvent : GameEvent { }
public class RewardedAdCompletedEvent : GameEvent { public bool Success; public bool RewardGiven; }
public class PaymentStartedEvent : GameEvent { public string OrderId; }
public class PaymentCompletedEvent : GameEvent { public string OrderId; public bool Success; }
```

---

## 接入状态

> ⚠️ 当前所有微信 API 均为框架实现，具体调用待接入微信 SDK。

| 功能 | 状态 |
|------|------|
| 登录 | 🟡 框架完成 |
| 分享 | 🟡 框架完成 |
| 激励广告 | 🟡 框架完成 |
| 支付 | 🟡 框架完成 |
| 云存储 | 🟡 框架完成 |
| 社交 | 🟡 框架完成 |
