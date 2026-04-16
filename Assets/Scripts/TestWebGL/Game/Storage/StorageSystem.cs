using System;
using System.Collections.Generic;
// V2: 暂时移除旧命名空间引用
// using TestWebGL.Game.Player;
// using TestWebGL.Game.Grid;

using SaveWorld.Game.Core;

namespace SaveWorld.Game.Storage
{
    /// <summary>
    /// 本地存储系统
    /// 负责游戏数据的保存和加载
    /// WebGL环境下使用JSON + localStorage
    /// </summary>
    public class StorageSystem
    {
        // 存储键名
        private const string PLAYER_DATA_KEY = "TestWebGL_PlayerData";
        private const string GRID_DATA_KEY = "TestWebGL_GridData";
        private const string GAME_SETTINGS_KEY = "TestWebGL_Settings";

        /// <summary>
        /// 游戏设置数据
        /// </summary>
        [System.Serializable]
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

        /// <summary>
        /// 网格存储数据
        /// </summary>
        [System.Serializable]
        public class GridSaveData
        {
            public int version = 1;
            public DateTime saveTime;
            public GridCellSaveData[] cells;

            [System.Serializable]
            public class GridCellSaveData
            {
                public int row;
                public int col;
                public bool isLocked;
                public int itemType;  // ItemType enum value
                public int itemCount;
                public int lockedItemType;  // for locked cells
                public int lockedItemLevel;
            }
        }

        /// <summary>
        /// 存储操作结果
        /// </summary>
        public enum StorageResult
        {
            Success = 0,
            FileNotFound = 1,
            CorruptedData = 2,
            VersionMismatch = 3,
            StorageUnavailable = 4,
            SerializationError = 5,
            UnknownError = 6
        }

        // 事件
        public delegate void SaveCompletedHandler(StorageResult result, string message);
        public event SaveCompletedHandler OnSaveCompleted;

        public delegate void LoadCompletedHandler(StorageResult result, string message);
        public event LoadCompletedHandler OnLoadCompleted;

        /// <summary>
        /// 保存完整游戏状态
        /// </summary>
        public StorageResult SaveGameState(GameState gameState)
        {
            try
            {
                var saveData = new GameStateSaveData
                {
                    version = 1,
                    saveTime = DateTime.UtcNow,
                    versionNumber = gameState.Version,
                    player = gameState.Player,
                    cells = gameState.Cells,
                    metadata = gameState.Metadata
                };

                string jsonData = UnityEngine.JsonUtility.ToJson(saveData);
                UnityEngine.PlayerPrefs.SetString(PLAYER_DATA_KEY, jsonData);
                UnityEngine.PlayerPrefs.Save();

                OnSaveCompleted?.Invoke(StorageResult.Success, $"游戏状态保存成功 版本:{gameState.Version}");
                return StorageResult.Success;
            }
            catch (Exception ex)
            {
                OnSaveCompleted?.Invoke(StorageResult.SerializationError, $"保存失败: {ex.Message}");
                return StorageResult.SerializationError;
            }
        }

        /// <summary>
        /// 加载完整游戏状态
        /// </summary>
        public (StorageResult result, GameStateSaveData data) LoadGameState()
        {
            try
            {
                if (!UnityEngine.PlayerPrefs.HasKey(PLAYER_DATA_KEY))
                {
                    OnLoadCompleted?.Invoke(StorageResult.FileNotFound, "未找到游戏存档");
                    return (StorageResult.FileNotFound, null);
                }

                string jsonData = UnityEngine.PlayerPrefs.GetString(PLAYER_DATA_KEY);

                if (string.IsNullOrEmpty(jsonData))
                {
                    OnLoadCompleted?.Invoke(StorageResult.CorruptedData, "存档数据为空");
                    return (StorageResult.CorruptedData, null);
                }

                GameStateSaveData saveData = UnityEngine.JsonUtility.FromJson<GameStateSaveData>(jsonData);

                if (saveData == null)
                {
                    OnLoadCompleted?.Invoke(StorageResult.CorruptedData, "存档反序列化失败");
                    return (StorageResult.CorruptedData, null);
                }

                if (saveData.version != 1)
                {
                    OnLoadCompleted?.Invoke(StorageResult.VersionMismatch, $"存档版本不匹配: {saveData.version}");
                    return (StorageResult.VersionMismatch, null);
                }

                OnLoadCompleted?.Invoke(StorageResult.Success, $"游戏存档加载成功 版本:{saveData.versionNumber} 保存时间:{saveData.saveTime}");
                return (StorageResult.Success, saveData);
            }
            catch (Exception ex)
            {
                OnLoadCompleted?.Invoke(StorageResult.SerializationError, $"加载失败: {ex.Message}");
                return (StorageResult.SerializationError, null);
            }
        }

        /// <summary>
        /// 游戏状态存档数据结构
        /// </summary>
        [System.Serializable]
        public class GameStateSaveData
        {
            public int version;
            public DateTime saveTime;
            public int versionNumber;
            public PlayerState player;
            public CellState[] cells;
            public Dictionary<string, string> metadata;
        }

        /// <summary>
        /// 加载网格数据
        /// </summary>
        public (StorageResult result, GridSaveData data) LoadGridData()
        {
            try
            {
                if (!UnityEngine.PlayerPrefs.HasKey(GRID_DATA_KEY))
                {
                    OnLoadCompleted?.Invoke(StorageResult.FileNotFound, "未找到网格存档");
                    return (StorageResult.FileNotFound, null);
                }

                string jsonData = UnityEngine.PlayerPrefs.GetString(GRID_DATA_KEY);

                if (string.IsNullOrEmpty(jsonData))
                {
                    OnLoadCompleted?.Invoke(StorageResult.CorruptedData, "网格数据为空");
                    return (StorageResult.CorruptedData, null);
                }

                GridSaveData gridData = UnityEngine.JsonUtility.FromJson<GridSaveData>(jsonData);

                if (gridData == null || gridData.cells == null)
                {
                    OnLoadCompleted?.Invoke(StorageResult.CorruptedData, "网格数据反序列化失败");
                    return (StorageResult.CorruptedData, null);
                }

                // 版本检查
                if (gridData.version != 1)
                {
                    OnLoadCompleted?.Invoke(StorageResult.VersionMismatch, $"网格数据版本不匹配: {gridData.version}");
                    return (StorageResult.VersionMismatch, null);
                }

                OnLoadCompleted?.Invoke(StorageResult.Success, "网格数据加载成功");
                return (StorageResult.Success, gridData);
            }
            catch (Exception ex)
            {
                OnLoadCompleted?.Invoke(StorageResult.SerializationError, $"网格数据反序列化失败: {ex.Message}");
                return (StorageResult.SerializationError, null);
            }
        }

        /// <summary>
        /// 保存游戏设置
        /// </summary>
        public StorageResult SaveGameSettings(GameSettings settings)
        {
            try
            {
                string jsonData = UnityEngine.JsonUtility.ToJson(settings);
                UnityEngine.PlayerPrefs.SetString(GAME_SETTINGS_KEY, jsonData);
                UnityEngine.PlayerPrefs.Save();

                OnSaveCompleted?.Invoke(StorageResult.Success, "游戏设置保存成功");
                return StorageResult.Success;
            }
            catch (Exception ex)
            {
                OnSaveCompleted?.Invoke(StorageResult.SerializationError, $"设置序列化失败: {ex.Message}");
                return StorageResult.SerializationError;
            }
        }

        /// <summary>
        /// 加载游戏设置
        /// </summary>
        public (StorageResult result, GameSettings settings) LoadGameSettings()
        {
            try
            {
                if (!UnityEngine.PlayerPrefs.HasKey(GAME_SETTINGS_KEY))
                {
                    // 返回默认设置
                    var defaultSettings = new GameSettings();
                    OnLoadCompleted?.Invoke(StorageResult.Success, "使用默认游戏设置");
                    return (StorageResult.Success, defaultSettings);
                }

                string jsonData = UnityEngine.PlayerPrefs.GetString(GAME_SETTINGS_KEY);
                GameSettings settings = UnityEngine.JsonUtility.FromJson<GameSettings>(jsonData);

                if (settings == null)
                {
                    settings = new GameSettings();
                    OnLoadCompleted?.Invoke(StorageResult.CorruptedData, "设置数据损坏，使用默认设置");
                    return (StorageResult.CorruptedData, settings);
                }

                OnLoadCompleted?.Invoke(StorageResult.Success, "游戏设置加载成功");
                return (StorageResult.Success, settings);
            }
            catch (Exception ex)
            {
                var defaultSettings = new GameSettings();
                OnLoadCompleted?.Invoke(StorageResult.SerializationError, $"设置反序列化失败，使用默认设置: {ex.Message}");
                return (StorageResult.SerializationError, defaultSettings);
            }
        }

        /// <summary>
        /// 删除所有存档数据
        /// </summary>
        public StorageResult DeleteAllSaveData()
        {
            try
            {
                UnityEngine.PlayerPrefs.DeleteKey(PLAYER_DATA_KEY);
                UnityEngine.PlayerPrefs.DeleteKey(GRID_DATA_KEY);
                UnityEngine.PlayerPrefs.DeleteKey(GAME_SETTINGS_KEY);
                UnityEngine.PlayerPrefs.Save();

                OnSaveCompleted?.Invoke(StorageResult.Success, "所有存档已删除");
                return StorageResult.Success;
            }
            catch (Exception ex)
            {
                OnSaveCompleted?.Invoke(StorageResult.UnknownError, $"删除存档失败: {ex.Message}");
                return StorageResult.UnknownError;
            }
        }

        /// <summary>
        /// 检查是否有存档
        /// </summary>
        public bool HasSaveData()
        {
            return UnityEngine.PlayerPrefs.HasKey(PLAYER_DATA_KEY) ||
                   UnityEngine.PlayerPrefs.HasKey(GRID_DATA_KEY);
        }

        /// <summary>
        /// 获取存储信息
        /// </summary>
        public string GetStorageInfo()
        {
            bool hasPlayerData = UnityEngine.PlayerPrefs.HasKey(PLAYER_DATA_KEY);
            bool hasGridData = UnityEngine.PlayerPrefs.HasKey(GRID_DATA_KEY);
            bool hasSettings = UnityEngine.PlayerPrefs.HasKey(GAME_SETTINGS_KEY);

            return $"存储状态: 玩家数据={hasPlayerData}, 网格数据={hasGridData}, 设置={hasSettings}";
        }
    }
}
