using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using SaveWorld.Game.Core;
using SaveWorld.Game.Items;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 背包UI
    /// 63格 9x7 背包显示
    /// </summary>
    public class BackpackUI : UIPanelBase
    {
        public Canvas ParentCanvas;
        public GridLayoutGroup GridLayout;
        public GameObject CellPrefab;

        private UICell[] _cells;
        private StateMutator _stateMutator;
        private EventBus _eventBus;
        private int _draggingCellId = -1;
        private Image _dragOverlayImage;
        private bool _initialized = false;

        void Awake()
        {
            if (CellPrefab == null)
            {
                var gridUI = GetComponent<GridUI>();
                if (gridUI != null) CellPrefab = gridUI.cellPrefab;
            }
            if (GridLayout == null)
                GridLayout = GetComponentInChildren<GridLayoutGroup>();
            if (ParentCanvas == null)
                ParentCanvas = GetComponentInParent<Canvas>();
        }

        public override void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _stateMutator = GameLoop.Instance.StateMutator;
            _eventBus = GameLoop.Instance.EventBus;

            if (GridLayout == null)
            {
                Debug.LogError("[BackpackUI] GridLayout is null, cannot create cells");
                return;
            }
            if (CellPrefab == null)
            {
                Debug.LogError("[BackpackUI] CellPrefab is null, cannot create cells");
                return;
            }

            // 创建63个格子
            _cells = new UICell[63];
            for (int i = 0; i < 63; i++)
            {
                var cellObj = Object.Instantiate(CellPrefab, GridLayout.transform);
                cellObj.name = "Cell_" + i;

                var uiCell = cellObj.GetComponent<UICell>();
                if (uiCell == null)
                    uiCell = cellObj.AddComponent<UICell>();

                // 从 GridCellUI 预制体组件获取引用
                var gridCellUI = cellObj.GetComponent<GridCellUI>();
                if (gridCellUI != null)
                {
                    uiCell.IconImage = gridCellUI.itemIcon;
                    uiCell.LevelText = gridCellUI.itemCountText;
                    uiCell.CellButton = gridCellUI.cellButton;
                }

                _cells[i] = uiCell;
                _cells[i].Initialize(i, _eventBus);
            }

            // 绑定事件总线
            _eventBus.Listen<MergeCompleteEvent>(OnMergeComplete);
            _eventBus.Listen<ItemMovedEvent>(OnItemMoved);
            _eventBus.Listen<ItemSwappedEvent>(OnItemSwapped);
            _eventBus.Listen<ExplorationCompleteEvent>(OnExplorationComplete);

            _eventBus.Listen<CellDragStartEvent>(OnCellDragStart);
            _eventBus.Listen<CellDragEndEvent>(OnCellDragEnd);

            // 创建全局拖拽遮罩层
            CreateDragOverlay();

            RefreshAll();
        }

        private void CreateDragOverlay()
        {
            if (ParentCanvas == null) return;

            GameObject overlayObj = new GameObject("DragOverlay");
            overlayObj.transform.SetParent(ParentCanvas.transform, false);
            overlayObj.transform.SetAsLastSibling();

            _dragOverlayImage = overlayObj.AddComponent<Image>();
            _dragOverlayImage.raycastTarget = false;
            _dragOverlayImage.color = new Color(1f, 1f, 1f, 0.8f);
            _dragOverlayImage.enabled = false;

            RectTransform rect = overlayObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 80);
        }

        private void OnCellDragStart(CellDragStartEvent e)
        {
            _draggingCellId = e.CellId;

            // 防御性检查
            if (_cells == null || e.CellId < 0 || e.CellId >= _cells.Length) return;
            var uiCell = _cells[e.CellId];
            if (uiCell == null || uiCell.IconImage == null) return;

            // 隐藏原格子图标
            uiCell.IconImage.enabled = false;

            // 复制图标到拖拽层
            var cellState = _stateMutator?.CurrentState?.Cells?[e.CellId];
            if (cellState.HasValue && cellState.Value.HasItem())
            {
                _dragOverlayImage.sprite = ItemIconManager.Instance.GetItemIcon((ItemType)cellState.Value.ItemId);
            }
            if (_dragOverlayImage != null)
                _dragOverlayImage.enabled = true;

            StartCoroutine(DragUpdateCoroutine());
        }

        private IEnumerator DragUpdateCoroutine()
        {
            while (_draggingCellId != -1)
            {
                if (_dragOverlayImage != null && _dragOverlayImage.enabled)
                {
                    var rect = _dragOverlayImage.GetComponent<RectTransform>();
                    rect.position = Mouse.current.position.ReadValue();
                }
                yield return null;
            }
        }

        private void OnCellDragEnd(CellDragEndEvent e)
        {
            // 防御性检查：确保拖拽是有效的
            if (_draggingCellId < 0 || _draggingCellId >= 63) return;
            if (_cells == null || _cells[_draggingCellId] == null) return;
            if (_cells[_draggingCellId].IconImage == null) return;

            // 检测目标格子
            int targetCellId = FindCellAtPosition(Mouse.current.position.ReadValue());

            if (targetCellId != -1 && targetCellId != _draggingCellId)
            {
                var cellState = _stateMutator?.CurrentState?.Cells?[targetCellId];
                if (!cellState.HasValue) return;
                var targetCell = cellState.Value;

                if (!targetCell.IsLocked)
                {
                    if (!targetCell.HasItem())
                    {
                        // 空格子 移动
                        _eventBus.Publish(new ItemMovedEvent(_draggingCellId, targetCellId, 0));
                    }
                    else
                    {
                        // 有物品 交换
                        var dragCellState = _stateMutator?.CurrentState?.Cells?[_draggingCellId];
                        if (dragCellState.HasValue)
                        {
                            _eventBus.Publish(new ItemSwappedEvent(
                                _draggingCellId, targetCellId,
                                dragCellState.Value.ItemId, targetCell.ItemId
                            ));
                        }
                    }
                }
            }

            // 恢复原格子图标
            if (_cells[_draggingCellId]?.IconImage != null)
                _cells[_draggingCellId].IconImage.enabled = true;

            // 隐藏拖拽层
            if (_dragOverlayImage != null)
                _dragOverlayImage.enabled = false;

            _draggingCellId = -1;
        }

        /// <summary>
        /// 检测鼠标位置下的格子
        /// </summary>
        private int FindCellAtPosition(Vector2 screenPosition)
        {
            for (int i = 0; i < 63; i++)
            {
                if (_cells[i] == null) continue;
                var rect = _cells[i].GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, null))
                {
                    return i;
                }
            }
            return -1;
        }

        private void OnMergeComplete(MergeCompleteEvent e)
        {
            // 合成完成: 刷新目标格子 + 播放动画
            RefreshCell(e.CellId);
            PlayMergeAnimation(e.CellId);
        }

        private void OnItemMoved(ItemMovedEvent e)
        {
            // 物品移动: 刷新源和目标两个格子
            RefreshCell(e.FromCellId);
            RefreshCell(e.ToCellId);
        }

        private void OnItemSwapped(ItemSwappedEvent e)
        {
            // 物品交换: 刷新两个格子
            RefreshCell(e.CellIdA);
            RefreshCell(e.CellIdB);
        }

        private void OnExplorationComplete(ExplorationCompleteEvent e)
        {
            // 探索完成: 刷新获得物品的格子
            foreach (int cellId in e.GeneratedCellIds)
            {
                RefreshCell(cellId);
            }
        }

        public override void Refresh()
        {
            RefreshAll();
        }

        /// <summary>
        /// 刷新所有格子
        /// </summary>
        public void RefreshAll()
        {
            if (_cells == null) return;
            for (int i = 0; i < 63; i++)
            {
                RefreshCell(i);
            }
        }

        /// <summary>
        /// 刷新单个格子
        /// </summary>
        public void RefreshCell(int cellId)
        {
            // 防御性编程：添加完整的空引用保护
            if (_cells == null) return;
            if (cellId < 0 || cellId >= 63) return;
            if (_cells[cellId] == null) return;
            if (_stateMutator?.CurrentState?.Cells == null) return;

            var cellState = _stateMutator.CurrentState.Cells[cellId];
            _cells[cellId].UpdateCell(cellState);
        }

        /// <summary>
        /// 播放合成动画
        /// </summary>
        public void PlayMergeAnimation(int cellId)
        {
            if (cellId >= 0 && cellId < 63 && _cells != null && _cells[cellId] != null)
            {
                _cells[cellId].PlayMergeEffect();
            }
        }
    }
}
