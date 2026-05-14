using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using SaveWorld.Game.Core;
using SaveWorld.Game.Items;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// UI格子单元
    /// 使用 IPointer 接口处理输入，减少 WebGL 运行时 EventTrigger 性能开销
    /// </summary>
    public class UICell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public int CellId;
        public Image IconImage;
        public TextMeshProUGUI LevelText;
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

            // 防御性检查：CellButton 可能未赋值
            if (CellButton == null)
            {
                Debug.LogWarning($"[UICell] CellButton is null for cell {cellId}");
            }
            // 不再使用 EventTrigger，直接通过 IPointer 接口处理输入
            // 保留 Button 用于视觉样式，但输入事件通过 IPointer 接口处理
        }

        // IPointerDownHandler 实现
        public void OnPointerDown(PointerEventData eventData)
        {
            if (IconImage == null || !IconImage.enabled) return;

            _pressStartTime = Time.unscaledTime;
            _longPressCoroutine = StartCoroutine(CheckLongPress());
        }

        // IPointerUpHandler 实现
        public void OnPointerUp(PointerEventData eventData)
        {
            if (_longPressCoroutine != null)
            {
                StopCoroutine(_longPressCoroutine);
                _longPressCoroutine = null;
            }

            if (_isDragging)
            {
                // 拖拽结束 发出拖拽完成事件
                _eventBus?.Publish(new CellDragEndEvent(this.CellId));
                _isDragging = false;
            }
        }

        // IPointerClickHandler 实现（替代 Button.onClick）
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging) return;

            float currentTime = Time.unscaledTime;

            if (currentTime - _lastClickTime < DOUBLE_CLICK_THRESHOLD)
            {
                // 双击 → 发出合成请求事件
                _eventBus?.Publish(new CellDoubleClickEvent(this.CellId));
                _lastClickTime = 0;
            }
            else
            {
                // 单击 → 发出选中事件
                _eventBus?.Publish(new CellClickEvent(this.CellId));
                _lastClickTime = currentTime;
            }
        }

        private IEnumerator CheckLongPress()
        {
            yield return new WaitForSecondsRealtime(LONG_PRESS_THRESHOLD);

            // 长按时间达到 开始拖拽
            _isDragging = true;

            // 发出拖拽开始事件
            _eventBus?.Publish(new CellDragStartEvent(this.CellId));
        }

        public void UpdateCell(CellState cellState)
        {
            // 防御性检查：IconImage 和 LevelText 可能未赋值
            if (IconImage == null || LevelText == null)
            {
                Debug.LogWarning($"[UICell] IconImage or LevelText is null for cell {CellId}");
                return;
            }

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
    }
}
