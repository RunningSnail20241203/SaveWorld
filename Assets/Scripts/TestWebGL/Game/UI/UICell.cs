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
    /// </summary>
    public class UICell : MonoBehaviour
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

            var trigger = CellButton.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = CellButton.gameObject.AddComponent<EventTrigger>();

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
