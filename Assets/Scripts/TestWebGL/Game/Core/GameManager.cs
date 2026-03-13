using UnityEngine;
using TestWebGL.Game.Grid;
using TestWebGL.Game.Player;
using TestWebGL.Game.Crafting;
using TestWebGL.Game.Feedback;
using TestWebGL.Game.Items;
using TestWebGL.Game.Exploration;
using TestWebGL.Game.Order;

namespace TestWebGL.Game.Core
{
    /// <summary>
    /// 游戏全局管理器
    /// 初始化和协调所有游戏系统
    /// 使用单例模式
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager s_instance;

        public static GameManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("GameManager");
                    s_instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // 各个系统的管理器
        private GridManager _gridManager;
        private PlayerManager _playerManager;
        private PlayerData _playerData;
        private CraftingSystem _craftingSystem;
        private CraftingEngine _craftingEngine;
        private FeedbackSystem _feedbackSystem;
        private ExplorationSystem _explorationSystem;
        private ExplorationEngine _explorationEngine;
        private OrderSystem _orderSystem;
        private OrderEngine _orderEngine;

        // 游戏状态
        private GameState _gameState = GameState.Initializing;

        // 事件
        public delegate void GameStateChangedHandler(GameState newState);
        public event GameStateChangedHandler OnGameStateChanged;

        public enum GameState
        {
            Initializing,
            Loading,
            Playing,
            Paused,
            GameOver,
        }

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化所有游戏系统
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[GameManager] 初始化游戏系统...");

            // 1. 加载或创建玩家数据
            _playerData = LoadPlayerData();
            if (_playerData == null)
            {
                _playerData = new PlayerData();
                Debug.Log("[GameManager] 创建新玩家存档");
            }
            else
            {
                Debug.Log($"[GameManager] 加载玩家存档: {_playerData.playerName} (Lv{_playerData.level})");
            }

            // 2. 初始化玩家管理器
            _playerManager = new PlayerManager();
            _playerManager.Initialize(_playerData);
            _playerManager.OnLevelChanged += HandlePlayerLevelChanged;
            _playerManager.OnStaminaChanged += HandlePlayerStaminaChanged;
            _playerManager.OnExperienceGained += HandleExperienceGained;
            Debug.Log("[GameManager] 玩家管理器已初始化");

            // 3. 初始化格子系统
            _gridManager = new GridManager();
            _gridManager.Initialize();
            _gridManager.OnCellChanged += HandleGridCellChanged;
            _gridManager.OnGridUnlocked += HandleGridUnlocked;
            Debug.Log("[GameManager] 格子系统已初始化");

            // 4. 初始化合成系统
            _craftingSystem = new CraftingSystem();
            _craftingEngine = new CraftingEngine();
            _craftingEngine.Initialize(_gridManager, _craftingSystem);
            _craftingEngine.OnCraftSuccess += HandleCraftSuccess;
            _craftingEngine.OnCraftFailure += HandleCraftFailure;
            _craftingEngine.OnFullGridFeedback += HandleFullGridFeedback;
            Debug.Log("[GameManager] 合成系统已初始化");

            // 5. 初始化反馈系统
            _feedbackSystem = FeedbackSystem.Instance;
            Debug.Log("[GameManager] 反馈系统已初始化");

            // 6. 初始化探索系统
            _explorationSystem = new ExplorationSystem();
            _explorationEngine = new ExplorationEngine(_explorationSystem, _gridManager, _playerManager);
            _explorationEngine.OnExploreResult += HandleExploreResult;
            _explorationEngine.OnPlacementFailed += HandlePlacementFailed;
            Debug.Log("[GameManager] 探索系统已初始化");

            // 7. 初始化订单系统
            _orderSystem = new OrderSystem();
            _orderEngine = new OrderEngine(_orderSystem, _gridManager, _playerManager);
            _orderEngine.OnOrderSubmitResult += HandleOrderSubmitResult;
            _orderEngine.OnRewardGranted += HandleRewardGranted;
            Debug.Log("[GameManager] 订单系统已初始化");

            // 8. 输出初始化统计
            var gridStats = _gridManager.GetStatistics();
            var gridDims = _gridManager.GetDimensions();
            Debug.Log($"[GameManager] 格子统计: 总={gridStats.totalCellCount}, 锁定={gridStats.lockedCellCount}, " +
                     $"已填={gridStats.filledCellCount}, 空={gridStats.emptyCellCount}");
            Debug.Log($"[GameManager] 格子尺寸: {gridDims.rows}×{gridDims.cols}, 每个格子{gridDims.cellSize}px, 间距{gridDims.cellSpacing}px");

            // 变更游戏状态
            ChangeGameState(GameState.Playing);

            Debug.Log("[GameManager] 游戏系统初始化完成!");
        }

        /// <summary>
        /// 变更游戏状态
        /// </summary>
        private void ChangeGameState(GameState newState)
        {
            if (_gameState != newState)
            {
                _gameState = newState;
                OnGameStateChanged?.Invoke(_gameState);
                Debug.Log($"[GameManager] 游戏状态: {_gameState}");
            }
        }

        /// <summary>
        /// 加载玩家数据（从本地存储）
        /// 第一阶段简单实现，后续接入存储系统
        /// </summary>
        private PlayerData LoadPlayerData()
        {
            // 临时实现：始终返回null，表示新游戏
            // 后期需要集成PlayerPrefs或其他存储方案
            return null;
        }

        /// <summary>
        /// 保存玩家数据
        /// </summary>
        public void SavePlayerData()
        {
            if (_playerData != null)
            {
                _playerData.lastSaveTime = System.DateTime.Now;
                // 后期实现具体的存储逻辑
                Debug.Log("[GameManager] 玩家数据已保存");
            }
        }

        // ==================== 获取各系统 ====================

        public GridManager GetGridManager() => _gridManager;
        public PlayerManager GetPlayerManager() => _playerManager;
        public PlayerData GetPlayerData() => _playerData;
        public CraftingSystem GetCraftingSystem() => _craftingSystem;
        public CraftingEngine GetCraftingEngine() => _craftingEngine;
        public FeedbackSystem GetFeedbackSystem() => _feedbackSystem;
        public ExplorationSystem GetExplorationSystem() => _explorationSystem;
        public ExplorationEngine GetExplorationEngine() => _explorationEngine;
        public OrderSystem GetOrderSystem() => _orderSystem;
        public OrderEngine GetOrderEngine() => _orderEngine;

        public GameState GetGameState() => _gameState;

        // ==================== 事件处理 ====================

        private void HandlePlayerLevelChanged(int newLevel, int oldLevel)
        {
            Debug.Log($"[GameManager] 玩家升级！ Lv{oldLevel} → Lv{newLevel}");
        }

        private void HandlePlayerStaminaChanged(int newStamina, int maxStamina)
        {
            Debug.Log($"[GameManager] 体力变化: {newStamina}/{maxStamina}");
        }

        private void HandleExperienceGained(int amount, string reason)
        {
            Debug.Log($"[GameManager] 获得 {amount} 经验值 ({reason})");
        }

        private void HandleGridCellChanged(int row, int col, Grid.GridCell cell)
        {
            Debug.Log($"[GameManager] 格子变化 [{row},{col}]: {cell.GetDisplayName()} ×{cell.ItemCount}");
        }

        private void HandleGridUnlocked(int row, int col, Grid.GridCell cell)
        {
            Debug.Log($"[GameManager] 格子解锁 [{row},{col}]!");
            // 触发反馈
            _feedbackSystem.UnlockSuccessFeedback();
        }

        private void HandleCraftSuccess(ItemType inputItem, int inputCount, ItemType outputItem, int outputCount)
        {
            Debug.Log($"[GameManager] 合成成功: {ItemConfig.GetItemName(inputItem)}×{inputCount} → {ItemConfig.GetItemName(outputItem)}×{outputCount}");
            _feedbackSystem.CraftSuccessFeedback();
            
            // 获得经验奖励（每合成1个物品获得1×物品等级经验）
            int expReward = outputCount * ItemConfig.GetItemLevel(outputItem);
            _playerManager.GainExperience(expReward, $"合成{ItemConfig.GetItemName(outputItem)}");
        }

        private void HandleCraftFailure(string reason)
        {
            Debug.Log($"[GameManager] 合成失败: {reason}");
            _feedbackSystem.CraftFailureFeedback();
        }

        private void HandleFullGridFeedback()
        {
            Debug.Log($"[GameManager] 满格提示！");
            _feedbackSystem.FullGridFlash();
        }

        private void HandleExploreResult(bool success, ItemType[] items, string message)
        {
            Debug.Log($"[GameManager] 探索结果: {(success ? "成功" : "失败")} - {message}");
            if (success && items != null)
            {
                _feedbackSystem.ExploreSuccessFeedback();
            }
            else
            {
                _feedbackSystem.CraftFailureFeedback();  // 暂时使用失败音效
            }
        }

        private void HandlePlacementFailed(string reason)
        {
            Debug.Log($"[GameManager] 物品放置失败: {reason}");
        }

        private void HandleOrderSubmitResult(bool success, OrderSystem.OrderData order, string message)
        {
            Debug.Log($"[GameManager] 订单提交结果: {(success ? "成功" : "失败")} - {message}");
            if (success)
            {
                _feedbackSystem.OrderCompleteFeedback();
            }
        }

        private void HandleRewardGranted(OrderSystem.RewardType rewardType, int amount)
        {
            Debug.Log($"[GameManager] 分发奖励: {rewardType} ×{amount}");
        }

        /// <summary>
        /// 调试：输出格子详细信息
        /// </summary>
        public void Debug_PrintGridInfo()
        {
            var dims = _gridManager.GetDimensions();
            Debug.Log($"\n========== 格子信息 ==========");
            Debug.Log($"尺寸: {dims.rows}×{dims.cols} ({dims.totalCells}格)");

            foreach (var cell in _gridManager.GetAllCells())
            {
                if (cell.IsLocked)
                {
                    Debug.Log($"[{cell.row},{cell.column}] 🔒 {cell.GetDisplayName()} Lv{cell.LockedItemLevel}");
                }
                else if (cell.HasItem)
                {
                    Debug.Log($"[{cell.row},{cell.column}] ✓ {cell.GetDisplayName()} ×{cell.ItemCount}");
                }
                else
                {
                    Debug.Log($"[{cell.row},{cell.column}] □ 空");
                }
            }

            var stats = _gridManager.GetStatistics();
            Debug.Log($"\n统计: 锁定={stats.lockedCellCount}, 已填={stats.filledCellCount}, 空={stats.emptyCellCount}");
        }

        /// <summary>
        /// 调试：输出玩家信息
        /// </summary>
        public void Debug_PrintPlayerInfo()
        {
            var stats = _playerManager.GetStatistics();
            Debug.Log($"\n========== 玩家信息 ==========");
            Debug.Log($"等级: {stats.level}");
            Debug.Log($"经验: {stats.experience}");
            Debug.Log($"体力: {stats.currentStamina}/{stats.maxStamina}");
            Debug.Log($"收集物品: {stats.collectedItemCount}");
            Debug.Log($"游戏时长: {stats.playTime.Hours}h {stats.playTime.Minutes}m");
        }

        /// <summary>
        /// 调试：消耗体力
        /// </summary>
        public void Debug_UseStamina(int amount = 1)
        {
            if (_playerManager.TryUseStamina(amount))
            {
                Debug.Log($"[DEBUG] 消耗 {amount} 体力");
            }
            else
            {
                Debug.Log($"[DEBUG] 体力不足!");
            }
        }

        /// <summary>
        /// 调试：获得经验
        /// </summary>
        public void Debug_GainExperience(int amount)
        {
            _playerManager.GainExperience(amount, "DEBUG");
            Debug.Log($"[DEBUG] 获得 {amount} 经验");
        }

        /// <summary>
        /// 调试：在格子中放置物品
        /// </summary>
        public void Debug_PlaceItem(int row, int col, Items.ItemType itemType, int count = 1)
        {
            var (success, errorCode) = _gridManager.TryPlaceItem(row, col, itemType, count);
            if (success)
            {
                Debug.Log($"[DEBUG] 在格子[{row},{col}]放置了 {Items.ItemConfig.GetItemName(itemType)} ×{count}");
            }
            else
            {
                Debug.Log($"[DEBUG] 放置失败: {GetPlacementErrorMessage(row, col, itemType, errorCode)}");
            }
        }

        /// <summary>
        /// 获取放置错误的可读消息
        /// </summary>
        private string GetPlacementErrorMessage(int row, int col, ItemType itemType, Grid.PlacementErrorCode errorCode)
        {
            return errorCode switch
            {
                Grid.PlacementErrorCode.InvalidPosition => $"格子位置[{row},{col}]无效",
                Grid.PlacementErrorCode.CellLocked => $"格子[{row},{col}]已锁定",
                Grid.PlacementErrorCode.StackLimitExceeded => $"格子[{row},{col}]堆叠数量超限",
                Grid.PlacementErrorCode.CellOccupied => $"格子[{row},{col}]已有其他物品",
                Grid.PlacementErrorCode.PlacementFailed => $"格子[{row},{col}]放置操作异常",
                _ => $"未知错误 ({errorCode})"
            };
        }

        /// <summary>
        /// 调试：双击合成
        /// </summary>
        public void Debug_DoubleTapCraft(int row, int col)
        {
            if (_craftingEngine.TryDoubleTapCraft(row, col))
            {
                Debug.Log($"[DEBUG] 在格子[{row},{col}]执行双击合成");
            }
            else
            {
                Debug.Log($"[DEBUG] 双击合成失败!");
            }
        }

        /// <summary>
        /// 调试：拖拽解锁
        /// </summary>
        public void Debug_DragToUnlock(int fromRow, int fromCol, int toRow, int toCol)
        {
            if (_craftingEngine.TryDragToUnlock(fromRow, fromCol, toRow, toCol))
            {
                Debug.Log($"[DEBUG] 成功解锁格子!");
            }
            else
            {
                Debug.Log($"[DEBUG] 解锁失败!");
            }
        }

        /// <summary>
        /// 调试：输出合成系统状态
        /// </summary>
        public void Debug_PrintCraftingInfo()
        {
            Debug.Log($"\n========== 合成系统状态 ==========");
            Debug.Log($"空格子数: {_craftingEngine.GetEmptyCellCount()}");
            Debug.Log($"已填充格子数: {_craftingEngine.GetFilledCellCount()}");
            Debug.Log($"格子满: {(_craftingEngine.IsGridFull() ? "是" : "否")}");
        }

        /// <summary>
        /// 调试：执行探索
        /// </summary>
        public void Debug_Explore()
        {
            _explorationEngine.TryExplore();
        }

        /// <summary>
        /// 调试：生成每日订单
        /// </summary>
        public void Debug_GenerateDailyOrders()
        {
            var playerData = _playerManager.GetPlayerData();
            _orderSystem.GenerateDailyOrders(playerData.level, playerData.historyMaxLevel);
        }

        /// <summary>
        /// 调试：提交订单
        /// </summary>
        public void Debug_SubmitOrder(int orderId)
        {
            _orderEngine.TrySubmitOrder(orderId);
        }

        private void OnDestroy()
        {
            // 清理事件订阅
            if (_playerManager != null)
            {
                _playerManager.OnLevelChanged -= HandlePlayerLevelChanged;
                _playerManager.OnStaminaChanged -= HandlePlayerStaminaChanged;
                _playerManager.OnExperienceGained -= HandleExperienceGained;
            }

            if (_craftingEngine != null)
            {
                _craftingEngine.OnCraftSuccess -= HandleCraftSuccess;
                _craftingEngine.OnCraftFailure -= HandleCraftFailure;
                _craftingEngine.OnFullGridFeedback -= HandleFullGridFeedback;
            }

            if (_explorationEngine != null)
            {
                _explorationEngine.OnExploreResult -= HandleExploreResult;
                _explorationEngine.OnPlacementFailed -= HandlePlacementFailed;
            }

            if (_orderEngine != null)
            {
                _orderEngine.OnOrderSubmitResult -= HandleOrderSubmitResult;
                _orderEngine.OnRewardGranted -= HandleRewardGranted;
            }
        }
    }
}
