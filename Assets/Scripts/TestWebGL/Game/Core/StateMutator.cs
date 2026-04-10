using System;

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

        public GameState CurrentState => _currentState;

        public StateMutator(EventBus eventBus, GameState initialState)
        {
            _eventBus = eventBus;
            _currentState = initialState;
            
            RegisterDefaultHandlers();
        }

        private void RegisterDefaultHandlers()
        {
            // 所有状态修改都在这里统一处理
            _eventBus.Listen<MergeCompleteEvent>(OnMergeComplete);
            _eventBus.Listen<ItemMovedEvent>(OnItemMoved);
            _eventBus.Listen<ItemSwappedEvent>(OnItemSwapped);
            _eventBus.Listen<ExplorationCompleteEvent>(OnExplorationComplete);
            _eventBus.Listen<LevelUpEvent>(OnLevelUp);
            _eventBus.Listen<OrderSubmittedEvent>(OnOrderSubmitted);
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
            var newCells = (CellState[])_currentState.Cells.Clone();
            var random = new Random();
            
            foreach (var cellId in e.GeneratedCellIds)
            {
                // 暂时生成物品ID 1，以后从配置表读取
                newCells[cellId] = CellState.Create(cellId, 1, 1);
            }

            var newPlayer = _currentState.Player;
            // 扣除体力
            
            UpdateState(newCells, newPlayer);
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

        private void UpdateState(CellState[] newCells, PlayerState newPlayer)
        {
            _currentState = new GameState(
                version: _currentState.Version + 1,
                cells: newCells,
                player: newPlayer,
                metadata: _currentState.Metadata
            );
        }
    }
}