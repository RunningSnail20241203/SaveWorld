# 存储系统 (Storage System)

> 文档版本: v2.2 | 最后更新: 2026-04-23
> 对应代码: `Assets/Scripts/TestWebGL/Game/Storage/`

---

## 概述

负责游戏数据的本地保存和加载。WebGL 环境下使用 JSON + PlayerPrefs (localStorage)。

---

## StorageSystem

**文件**: `Storage/StorageSystem.cs` | **命名空间**: `SaveWorld.Game.Storage`

### 职责
- 游戏状态完整序列化/反序列化
- 游戏设置持久化
- 存档版本检查
- 错误处理与回调

### 存储键名

```csharp
private const string PLAYER_DATA_KEY = "TestWebGL_PlayerData";
private const string GRID_DATA_KEY = "TestWebGL_GridData";
private const string GAME_SETTINGS_KEY = "TestWebGL_Settings";
```

### 核心方法

```csharp
// 保存/加载完整游戏状态
public StorageResult SaveGameState(GameState gameState)
public (StorageResult result, GameStateSaveData data) LoadGameState()

// 保存/加载游戏设置
public StorageResult SaveGameSettings(GameSettings settings)
public (StorageResult result, GameSettings settings) LoadGameSettings()

// 加载网格数据（旧版兼容）
public (StorageResult result, GridSaveData data) LoadGridData()

// 删除所有存档
public StorageResult DeleteAllSaveData()

// 查询
public bool HasSaveData()
public string GetStorageInfo()
```

### StorageResult 枚举

| 值 | 说明 |
|-----|------|
| `Success = 0` | 成功 |
| `FileNotFound = 1` | 文件未找到 |
| `CorruptedData = 2` | 数据损坏 |
| `VersionMismatch = 3` | 版本不匹配 |
| `StorageUnavailable = 4` | 存储不可用 |
| `SerializationError = 5` | 序列化错误 |
| `UnknownError = 6` | 未知错误 |

---

## 存档数据结构

### GameStateSaveData

```csharp
[Serializable]
public class GameStateSaveData
{
    public int version;              // 存档格式版本
    public DateTime saveTime;        // 保存时间
    public int versionNumber;        // 游戏状态版本号
    public PlayerState player;       // 玩家状态
    public CellState[] cells;        // 格子状态
    public IReadOnlyDictionary<string, object> metadata;
}
```

### GameSettings

```csharp
[Serializable]
public class GameSettings
{
    public float masterVolume = 1.0f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1.0f;
    public bool vibrationEnabled = true;
    public bool soundEnabled = true;
    public string language = "zh-CN";
    public bool autoSaveEnabled = true;
    public int autoSaveIntervalMinutes = 5;
}
```

### GridSaveData (旧版兼容)

```csharp
[Serializable]
public class GridSaveData
{
    public int version = 1;
    public DateTime saveTime;
    public GridCellSaveData[] cells;

    [Serializable]
    public class GridCellSaveData
    {
        public int row;
        public int col;
        public bool isLocked;
        public int itemType;
        public int itemCount;
        public int lockedItemType;
        public int lockedItemLevel;
    }
}
```

---

## 自动保存

由 `StateMutator` 管理：
- 间隔: 300 秒 (5 分钟)
- 触发条件: 每次状态变更后检查
- 方法: `CheckAutoSave()` → `SaveCurrentState()`

---

## CloudStorageSystem

**文件**: `Storage/CloudStorageSystem.cs`

### 职责
微信云存储接口封装，支持存档上传/下载/同步。

```csharp
public void UploadSaveData(string data)     // 上传存档
public string DownloadSaveData()            // 下载存档
public void SyncWithCloud()                 // 同步云端存档
```
