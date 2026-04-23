# 其他系统汇总

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/*System/`

---

## 成就系统 (Achievement System)

**文件**: `Achievement/AchievementSystem.cs` | **命名空间**: `SaveWorld.Game.Achievement`

### 职责
成就进度跟踪、解锁判定、奖励发放。

### 核心类

```csharp
public class AchievementSystem
{
    public AchievementSystem(EventBus eventBus, StateMutator stateMutator)
    public void InitializeAchievements()
    private void UpdateAchievementProgress(AchievementType type, int value)
    private bool IsAchievementUnlocked(int achievementId)
    private bool CanUnlockAchievement(int achievementId)
    private void UnlockAchievement(int achievementId)
}

public class AchievementData
{
    public int AchievementId;
    public string Name;
    public string Description;
    public AchievementType Type;
    public int TargetValue;
    public int CurrentValue;
    public bool IsUnlocked;
    public long UnlockTime;
}

public enum AchievementType
{
    CraftCount,        // 合成次数
    ExploreCount,      // 探索次数
    OrderCount,        // 完成订单数
    LevelReached,      // 达到等级
    ItemCollected,     // 收集物品
    GridUnlocked       // 解锁格子
}
```

### 监听事件
- `ItemCraftedEvent` → 更新合成成就
- `ExplorationCompleteEvent` → 更新探索成就
- `OrderSubmittedEvent` → 更新订单成就
- `LevelUpEvent` → 更新等级成就
- `ExperienceGainedEvent` → 更新经验成就

---

## 音频系统 (Audio System)

**文件**: `Audio/AudioManager.cs` | **命名空间**: `SaveWorld.Game.Audio`

### 职责
背景音乐与音效播放、音量控制。

### 核心方法

```csharp
public AudioManager(EventBus eventBus)

public void PlayMusic(MusicType type)
public void StopMusic()
public void PlaySFX(SoundType type)

public void SetMasterVolume(float volume)
public void SetMusicVolume(float volume)
public void SetSFXVolume(float volume)

public void ToggleMusic(bool enabled)
public void ToggleSFX(bool enabled)
```

### 类型定义

```csharp
public enum MusicType
{
    MainMenu, Gameplay, Exploration, Boss
}

public enum SoundType
{
    ButtonClick, MergeSuccess, MergeFail, 
    ExploreStart, ExploreComplete, LevelUp,
    OrderComplete, AchievementUnlock, Error
}
```

---

## 数据分析系统 (Analytics System)

**文件**: `Analytics/AnalyticsSystem.cs` | **命名空间**: `SaveWorld.Game.Analytics`

### 职责
游戏数据统计与上报。

### 核心方法

```csharp
public AnalyticsSystem(EventBus eventBus)

public void TrackEvent(string eventName, Dictionary<string, object> parameters)
public void TrackPlayerAction(string action, int value)
public void TrackLevelProgress(int level, int exp)
public void TrackEconomy(string currencyType, long amount, string reason)
public void FlushEvents()              // 立即上报
```

### AnalyticsEvent 结构

```csharp
public struct AnalyticsEvent
{
    public string EventName;
    public Dictionary<string, object> Parameters;
    public long Timestamp;
    public string SessionId;
}
```

---

## 社交系统 (Social System)

**文件**: `Social/SocialSystem.cs` | **命名空间**: `SaveWorld.Game.Social`

### 职责
好友管理、排行榜、社交互动。

### 核心方法

```csharp
public SocialSystem(EventBus eventBus)

public void LoadFriendList()
public void GetLeaderboard(string type, int count)
public void SendGift(string friendId, ItemType itemType)
public void RequestHelp(string helpType)
```

---

## 反馈系统 (Feedback System)

**文件**: `Feedback/FeedbackSystem.cs` | **命名空间**: `SaveWorld.Game.Feedback`

### 职责
玩家反馈收集、Bug 上报、客服系统。

### 核心方法

```csharp
public FeedbackSystem(EventBus eventBus)

public void SubmitFeedback(string content, string contact = "")
public void SubmitBugReport(string description, string screenshot = "")
public void OpenCustomerService()
```

---

## 本地化系统 (Localization)

**文件**: `Localization/LocalizationManager.cs` | **命名空间**: `SaveWorld.Game.Localization`

### 职责
多语言支持。

### 核心方法

```csharp
public string GetText(string key)
public void SetLanguage(string languageCode)
public void LoadLanguagePack(string languageCode)
```

---

## 各系统状态汇总

| 系统 | 状态 | 说明 |
|------|------|------|
| 成就系统 | ✅ 完成 | 完整实现 |
| 音频系统 | ✅ 完成 | 完整实现 |
| 数据分析 | ✅ 完成 | 完整实现 |
| 社交系统 | ✅ 完成 | 框架完成 |
| 反馈系统 | ✅ 完成 | 框架完成 |
| 本地化 | 🟡 框架 | 基础框架 |
