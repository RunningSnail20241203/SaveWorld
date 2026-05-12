using System;
using System.Collections.Generic;
using UnityEngine;
using SaveWorld.Game.Order;
using SaveWorld.Game.Achievement;
using SaveWorld.Game.Exploration;
using SaveWorld.Game.Storage;

namespace SaveWorld.Game.Core
{
    /// <summary>
    /// 状态修改器
    /// 唯一允许修改GameState的地方
    /// 所有状态修改都必须通过这里进行
    /// </summary>
    public sealed class StateMutator
    {
        private readonly EventBus _eventBus;
        private GameState _currentState;
        private SaveWorld.Game.Storage.StorageSystem _storageSystem;
        private DateTime _lastAutoSaveTime;
        private const int AUTO_SAVE_INTERVAL_SECONDS = 300; // 5分钟自动保存

        public GameState CurrentState => _currentState;

        public StateMutator(EventBus eventBus, GameState initialState, SaveWorld.Game.Storage.StorageSystem storageSystem)
        {
            _eventBus = eventBus;
            _currentState = initialState;
            _storageSystem = storageSystem;
            _lastAutoSaveTime = DateTime.UtcNow;
            
            RegisterDefaultHandlers();
        }

        private void RegisterDefaultHandlers()
        {
            // 所有状态修改都在这里统一处理
            _eventBus.Listen<ExplorationRequestEvent>(OnExplorationRequest);
            _eventBus.Listen<MergeCompleteEvent>(OnMergeComplete);
            _eventBus.Listen<ItemMovedEvent>(OnItemMoved);
            _eventBus.Listen<ItemSwappedEvent>(OnItemSwapped);
            _eventBus.Listen<ExplorationCompleteEvent>(OnExplorationComplete);
            _eventBus.Listen<LevelUpEvent>(OnLevelUp);
            _eventBus.Listen<OrderSubmittedEvent>(OnOrderSubmitted);
        }

        private void OnExplorationRequest(ExplorationRequestEvent e)
        {
            int randomSeed = UnityEngine.Random.Range(0, int.MaxValue);
            var result = ExplorationEngine.TryExplore(_currentState, randomSeed);

            if (!result.Success)
            {
                Debug.LogWarning($"[StateMutator] 探索失败: {result.FailReason}");
                return;
            }

            // 更新格子：放入探索获得的物品
            var newCells = (CellState[])_currentState.Cells.Clone();
            for (int i = 0; i < result.CellIds.Length; i++)
            {
                int cellId = result.CellIds[i];
                int itemId = result.ItemIds[i];
                newCells[cellId] = CellState.Create(cellId, itemId, 1);
            }

            // 扣除体力
            var player = _currentState.Player;
            var newPlayer = new PlayerState(
                level: player.Level,
                stamina: player.Stamina - e.StaminaCost,
                maxStamina: player.MaxStamina,
                gold: player.Gold,
                coins: player.Coins,
                exp: player.Exp,
                experience: player.Experience + result.ExperienceGain,
                expToNextLevel: player.ExpToNextLevel,
                lastOfflineTime: player.LastOfflineTime
            );

            UpdateState(newCells, newPlayer);

            // 发布探索完成事件
            _eventBus.Publish(new ExplorationCompleteEvent(result.CellIds, e.StaminaCost));
            _eventBus.Publish(new ExperienceGainedEvent(result.ExperienceGain, "探索"));

            Debug.Log($"[StateMutator] 探索成功: 获得{result.CellIds.Length}个物品, 消耗{e.StaminaCost}体力, 获得{result.ExperienceGain}经验");
        }

        private void OnMergeComplete(MergeCompleteEvent e)
        {
            // 创建新的状态副本
            var newCells = (CellState[])_currentState.Cells.Clone();
            
            // 清除两个旧格子
            newCells[e.CellId] = CellState.Empty(e.CellId);
            
            // 创建新物品
            newCells[e.CellId] = CellState.Create(e.CellId, e.NewItemId, 1);
            
            UpdateState(newCells, _currentState.Player);
        }

        private void OnItemMoved(ItemMovedEvent e)
        {
            var newCells = (CellState[])_currentState.Cells.Clone();
            
            var item = newCells[e.FromCellId];
            newCells[e.FromCellId] = CellState.Empty(e.FromCellId);
            newCells[e.ToCellId] = CellState.Create(e.ToCellId, item.ItemId, item.Count);
            
            UpdateState(newCells, _currentState.Player);
        }

        private void OnItemSwapped(ItemSwappedEvent e)
        {
            var newCells = (CellState[])_currentState.Cells.Clone();
            
            var itemA = newCells[e.CellIdA];
            var itemB = newCells[e.CellIdB];
            
            newCells[e.CellIdA] = CellState.Create(e.CellIdA, itemB.ItemId, itemB.Count);
            newCells[e.CellIdB] = CellState.Create(e.CellIdB, itemA.ItemId, itemA.Count);
            
            UpdateState(newCells, _currentState.Player);
        }

        private void OnExplorationComplete(ExplorationCompleteEvent e)
        {
            // 状态更新已在 OnExplorationRequest 中处理
            // UI 刷新由 BackpackUI / PlayerStatusUI 监听此事件自行处理
        }

        private void OnLevelUp(LevelUpEvent e)
        {
            var newPlayer = _currentState.Player;
            UpdateState(_currentState.Cells, newPlayer);
        }

        private void OnOrderSubmitted(OrderSubmittedEvent e)
        {
            var newPlayer = _currentState.Player;
            UpdateState(_currentState.Cells, newPlayer);
        }

        /// <summary>
        /// 更新状态（格子+玩家）
        /// </summary>
        internal void UpdateState(CellState[] newCells, PlayerState newPlayer)
        {
            _currentState = new GameState(
                version: _currentState.Version + 1,
                cells: newCells,
                player: newPlayer,
                orders: _currentState.Orders,
                lastOrderResetDate: _currentState.LastOrderResetDate,
                achievements: _currentState.Achievements,
                metadata: _currentState.Metadata
            );

            // 自动保存检查
            CheckAutoSave();
        }

        /// <summary>
        /// 更新状态（格子+玩家+订单）
        /// </summary>
        internal void UpdateState(CellState[] newCells, PlayerState newPlayer, 
                                  IReadOnlyDictionary<int, SaveWorld.Game.Order.OrderData> newOrders)
        {
            _currentState = new GameState(
                version: _currentState.Version + 1,
                cells: newCells,
                player: newPlayer,
                orders: newOrders,
                lastOrderResetDate: _currentState.LastOrderResetDate,
                achievements: _currentState.Achievements,
                metadata: _currentState.Metadata
            );

            CheckAutoSave();
        }

        /// <summary>
        /// 更新状态（格子+玩家+订单+成就）
        /// </summary>
        internal void UpdateState(CellState[] cells, PlayerState player, 
                                  IReadOnlyDictionary<int, SaveWorld.Game.Order.OrderData> orders,
                                  IReadOnlyDictionary<int, SaveWorld.Game.Achievement.AchievementData> achievements)
        {
            _currentState = new GameState(
                version: _currentState.Version + 1,
                cells: cells,
                player: player,
                orders: orders,
                lastOrderResetDate: _currentState.LastOrderResetDate,
                achievements: achievements,
                metadata: _currentState.Metadata
            );

            CheckAutoSave();
        }

        /// <summary>
        /// 更新状态（全字段）
        /// </summary>
        internal void UpdateState(CellState[] cells, PlayerState player, 
                                  IReadOnlyDictionary<int, SaveWorld.Game.Order.OrderData> orders,
                                  DateTime lastOrderResetDate,
                                  IReadOnlyDictionary<int, SaveWorld.Game.Achievement.AchievementData> achievements)
        {
            _currentState = new GameState(
                version: _currentState.Version + 1,
                cells: cells,
                player: player,
                orders: orders,
                lastOrderResetDate: lastOrderResetDate,
                achievements: achievements,
                metadata: _currentState.Metadata
            );

            CheckAutoSave();
        }

        /// <summary>
        /// 检查是否需要自动保存
        /// </summary>
        private void CheckAutoSave()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastAutoSaveTime).TotalSeconds >= AUTO_SAVE_INTERVAL_SECONDS)
            {
                SaveCurrentState();
                _lastAutoSaveTime = now;
            }
        }

        /// <summary>
        /// 立即保存当前状态
        /// </summary>
        public void SaveCurrentState()
        {
            _storageSystem.SaveGameState(_currentState);
        }

        /// <summary>
        /// 加载存档状态
        /// </summary>
        public bool LoadSavedState()
        {
            var (result, saveData) = _storageSystem.LoadGameState();
            // StorageSystem 旧版本不包含 orders/achievements 字段，加载时使用默认值
            if (result == StorageSystem.StorageResult.Success && saveData != null)
            {
                _currentState = new GameState(
                    version: saveData.versionNumber,
                    cells: saveData.cells,
                    player: saveData.player,
                    orders: saveData.orders ?? new Dictionary<int, SaveWorld.Game.Order.OrderData>(),
                    lastOrderResetDate: saveData.lastOrderResetDate,
                    achievements: saveData.achievements ?? new Dictionary<int, SaveWorld.Game.Achievement.AchievementData>(),
                    metadata: saveData.metadata
                );

                // 计算离线体力恢复
                const int MAX_STAMINA = 100;
                int offlineSeconds = (int)(DateTime.UtcNow - saveData.saveTime).TotalSeconds;
                int recoveredStamina = _currentState.CalculateOfflineRecoveredStamina(MAX_STAMINA, 300);
                
                if (recoveredStamina > 0)
                {
                    var newPlayer = new PlayerState(
                        level: _currentState.Player.Level,
                        stamina: Math.Min(MAX_STAMINA, _currentState.Player.Stamina + recoveredStamina),
                        maxStamina: _currentState.Player.MaxStamina,
                        gold: _currentState.Player.Gold,
                        coins: _currentState.Player.Coins,
                        exp: _currentState.Player.Exp,
                        experience: _currentState.Player.Experience,
                        expToNextLevel: _currentState.Player.ExpToNextLevel,
                        lastOfflineTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    );
                    
                    _currentState = new GameState(
                        _currentState.Version, 
                        _currentState.Cells, 
                        newPlayer, 
                        _currentState.Orders,
                        _currentState.LastOrderResetDate,
                        _currentState.Achievements,
                        _currentState.Metadata
                    );
                }

                // TODO: GameStateLoadedEvent 事件待实现
                // _eventBus.Publish(new GameStateLoadedEvent { LoadedState = _currentState });
                return true;
            }
            return false;
        }
    }
}