using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        [Header("背包配置")]
        public Canvas ParentCanvas;
        public GridLayoutGroup GridLayout;
        public GameObject CellPrefab;

        private UICell[] _cells;
        private StateMutator _stateMutator;
        private EventBus _eventBus;
        private int _draggingCellId = -1;
        private Image _dragOverlayImage;

        public override void Initialize()
        {
            _stateMutator = GameLoop.Instance.StateMutator;
            _eventBus = GameLoop.Instance.EventBus;

            // 创建63个格子
            _cells = new UICell[63];
            for (int i = 0; i < 63; i++)
            {
                var cellObj = UnityEngine.Object.Instantiate(CellPrefab, GridLayout.transform);
                _cells[i] = cellObj.GetComponent<UICell>();
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

            // 隐藏原格子图标
            _cells[e.CellId].IconImage.enabled = false;

            // 复制图标到拖拽层
            var cellState = _stateMutator.CurrentState.Cells[e.CellId];
            if (cellState.HasItem())
            {
                _dragOverlayImage.sprite = Items.ItemIconManager.Instance.GetItemIcon((ItemType)cellState.ItemId);
            }
            _dragOverlayImage.enabled = true;

            // TODO: V2迁移 - StartCoroutine 需要 MonoBehaviour
            // StartCoroutine(DragUpdateCoroutine());
        }

        private void OnCellDragEnd(CellDragEndEvent e)
        {
            // 检测目标格子
            int targetCellId = FindCellAtPosition(Input.mousePosition);

            if (targetCellId != -1 && targetCellId != _draggingCellId)
            {
                var cellState = _stateMutator.CurrentState.Cells[targetCellId];

                if (!cellState.IsLocked)
                {
                    if (!cellState.HasItem())
                    {
                        // 空格子 移动
                        _eventBus.Publish(new ItemMovedEvent(_draggingCellId, targetCellId, 0));
                    }
                    else
                    {
                        // 有物品 交换
                        var dragCellState = _stateMutator.CurrentState.Cells[_draggingCellId];
                        _eventBus.Publish(new ItemSwappedEvent(
                            _draggingCellId, targetCellId,
                            dragCellState.ItemId, cellState.ItemId
                        ));
                    }
                }
            }

            // 恢复原格子图标
            _cells[_draggingCellId].IconImage.enabled = true;

            // 隐藏拖拽层
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
            if (cellId < 0 || cellId >= 63) return;

            var cellState = _stateMutator.CurrentState.Cells[cellId];
            _cells[cellId].UpdateCell(cellState);
        }

        /// <summary>
        /// 播放合成动画
        /// </summary>
        public void PlayMergeAnimation(int cellId)
        {
            if (cellId >= 0 && cellId < 63)
            {
                _cells[cellId].PlayMergeEffect();
            }
        }
    }

    /// <summary>
    /// UI格子单元
    /// </summary>
    public class UICell : MonoBehaviour
    {
        public int CellId;
        public Image IconImage;
        public Text LevelText;
        public Button CellButton;

        private EventBus _eventBus;
        private float _lastClickTime;
        private const float DOUBLE_CLICK_THRESHOLD = 0.3f;
        private const float LONG_PRESS_THRESHOLD = 0.4f;
        private bool _isDragging;
        private float _pressStartTime;
        private Coroutine _longPressCoroutine;

        public void Initialize(int cellId, EventBus eventBus)
        {
            CellId = cellId;
            _eventBus = eventBus;

            var trigger = CellButton.GetComponent<EventTrigger>();

            // 按下事件
            EventTrigger.Entry pressEntry = new EventTrigger.Entry();
            pressEntry.eventID = EventTriggerType.PointerDown;
            pressEntry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
            trigger.triggers.Add(pressEntry);

            // 抬起事件
            EventTrigger.Entry releaseEntry = new EventTrigger.Entry();
            releaseEntry.eventID = EventTriggerType.PointerUp;
            releaseEntry.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
            trigger.triggers.Add(releaseEntry);

            // 点击事件
            CellButton.onClick.AddListener(OnCellClicked);
        }

        public void UpdateCell(CellState cellState)
        {
            if (cellState.HasItem())
            {
                IconImage.sprite = Items.ItemIconManager.Instance.GetItemIcon((ItemType)cellState.ItemId);
                IconImage.enabled = true;
                LevelText.text = "L" + Items.ItemConfig.GetItemLevel((ItemType)cellState.ItemId);
                LevelText.enabled = true;
            }
            else
            {
                IconImage.enabled = false;
                LevelText.enabled = false;
            }
        }

        public void PlayMergeEffect()
        {
            // 播放升级动画
            GetComponent<Animator>()?.SetTrigger("Merge");
        }

        private void OnPointerDown(PointerEventData eventData)
        {
            if (!IconImage.enabled) return;

            _pressStartTime = Time.unscaledTime;
            _longPressCoroutine = StartCoroutine(CheckLongPress());
        }

        private void OnPointerUp(PointerEventData eventData)
        {
            if (_longPressCoroutine != null)
            {
                StopCoroutine(_longPressCoroutine);
                _longPressCoroutine = null;
            }

            if (_isDragging)
            {
                // 拖拽结束 发出拖拽完成事件
                _eventBus.Publish(new CellDragEndEvent(this.CellId));

                _isDragging = false;
            }
        }

        private IEnumerator CheckLongPress()
        {
            yield return new WaitForSecondsRealtime(LONG_PRESS_THRESHOLD);

            // 长按时间达到 开始拖拽
            _isDragging = true;

            // 发出拖拽开始事件
            _eventBus.Publish(new CellDragStartEvent(this.CellId));
        }

        private void OnCellClicked()
        {
            if (_isDragging) return;

            float currentTime = Time.unscaledTime;

            if (currentTime - _lastClickTime < DOUBLE_CLICK_THRESHOLD)
            {
                // 双击 → 发出合成请求事件
                _eventBus.Publish(new CellDoubleClickEvent(this.CellId));

                _lastClickTime = 0;
            }
            else
            {
                // 单击 → 发出选中事件
                _eventBus.Publish(new CellClickEvent(this.CellId));

                _lastClickTime = currentTime;
            }
        }
    }
}
