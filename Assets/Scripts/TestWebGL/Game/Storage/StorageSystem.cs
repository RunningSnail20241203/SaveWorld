using System;
using System.Collections.Generic;
using TestWebGL.Game.Player;
using TestWebGL.Game.Grid;

namespace TestWebGL.Game.Storage
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
        /// 保存玩家数据
        /// </summary>
        public StorageResult SavePlayerData(PlayerData playerData)
        {
            try
            {
                if (playerData == null)
                    return StorageResult.UnknownError;

                // 更新保存时间
                playerData.lastSaveTime = DateTime.Now;

                // 序列化为JSON
                string jsonData = UnityEngine.JsonUtility.ToJson(playerData);

                // 保存到本地存储
                UnityEngine.PlayerPrefs.SetString(PLAYER_DATA_KEY, jsonData);
                UnityEngine.PlayerPrefs.Save();

                OnSaveCompleted?.Invoke(StorageResult.Success, "玩家数据保存成功");
                return StorageResult.Success;
            }
            catch (Exception ex)
            {
                OnSaveCompleted?.Invoke(StorageResult.SerializationError, $"序列化失败: {ex.Message}");
                return StorageResult.SerializationError;
            }
        }

        /// <summary>
        /// 加载玩家数据
        /// </summary>
        public (StorageResult result, PlayerData data) LoadPlayerData()
        {
            try
            {
                if (!UnityEngine.PlayerPrefs.HasKey(PLAYER_DATA_KEY))
                {
                    OnLoadCompleted?.Invoke(StorageResult.FileNotFound, "未找到玩家存档");
                    return (StorageResult.FileNotFound, null);
                }

                string jsonData = UnityEngine.PlayerPrefs.GetString(PLAYER_DATA_KEY);

                if (string.IsNullOrEmpty(jsonData))
                {
                    OnLoadCompleted?.Invoke(StorageResult.CorruptedData, "玩家数据为空");
                    return (StorageResult.CorruptedData, null);
                }

                PlayerData playerData = UnityEngine.JsonUtility.FromJson<PlayerData>(jsonData);

                if (playerData == null)
                {
                    OnLoadCompleted?.Invoke(StorageResult.CorruptedData, "玩家数据反序列化失败");
                    return (StorageResult.CorruptedData, null);
                }

                OnLoadCompleted?.Invoke(StorageResult.Success, "玩家数据加载成功");
                return (StorageResult.Success, playerData);
            }
            catch (Exception ex)
            {
                OnLoadCompleted?.Invoke(StorageResult.SerializationError, $"反序列化失败: {ex.Message}");
                return (StorageResult.SerializationError, null);
            }
        }

        /// <summary>
        /// 保存网格数据
        /// </summary>
        public StorageResult SaveGridData(GridManager gridManager)
        {
            try
            {
                var saveData = new GridSaveData
                {
                    version = 1,
                    saveTime = DateTime.Now,
                    cells = new GridSaveData.GridCellSaveData[63]  // 9x7 = 63
                };

                int index = 0;
                foreach (var cell in gridManager.GetAllCells())
                {
                    saveData.cells[index] = new GridSaveData.GridCellSaveData
                    {
                        row = cell.row,
                        col = cell.column,
                        isLocked = cell.IsLocked,
                        itemType = cell.HasItem ? (int)cell.CurrentItemType : 0,
                        itemCount = cell.ItemCount,
                        lockedItemType = cell.IsLocked ? (int)cell.LockedItemType : 0,
                        lockedItemLevel = cell.IsLocked ? cell.LockedItemLevel : 0
                    };
                    index++;
                }

                string jsonData = UnityEngine.JsonUtility.ToJson(saveData);
                UnityEngine.PlayerPrefs.SetString(GRID_DATA_KEY, jsonData);
                UnityEngine.PlayerPrefs.Save();

                OnSaveCompleted?.Invoke(StorageResult.Success, "网格数据保存成功");
                return StorageResult.Success;
            }
            catch (Exception ex)
            {
                OnSaveCompleted?.Invoke(StorageResult.SerializationError, $"网格数据序列化失败: {ex.Message}");
                return StorageResult.SerializationError;
            }
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
