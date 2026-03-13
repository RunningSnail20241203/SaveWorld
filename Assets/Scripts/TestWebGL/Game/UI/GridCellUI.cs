using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestWebGL.Game.Grid;
using TestWebGL.Game.Items;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// 格子UI - 单个网格格子的UI显示和交互
    /// 显示物品信息，支持点击交互
    /// </summary>
    public class GridCellUI : MonoBehaviour
    {
        [Header("UI组件")]
        public Image backgroundImage;
        public Image itemIcon;
        public TextMeshProUGUI itemCountText;
        public TextMeshProUGUI lockLevelText;
        public Button cellButton;

        [Header("视觉状态")]
        public Color normalColor = Color.white;
        public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        public Color filledColor = new Color(1f, 1f, 1f, 0.9f);

        // 格子位置
        private int _row;
        private int _col;
        private GridCell _currentCell;

        /// <summary>
        /// 初始化格子UI
        /// </summary>
        public void Initialize(int row, int col)
        {
            _row = row;
            _col = col;

            // 创建UI组件
            CreateUIComponents();

            // 设置按钮事件
            if (cellButton != null)
            {
                cellButton.onClick.AddListener(OnCellClicked);
            }
        }

        /// <summary>
        /// 创建UI组件
        /// </summary>
        private void CreateUIComponents()
        {
            // 获取或创建RectTransform
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            // 创建背景图片
            if (backgroundImage == null)
            {
                GameObject bgGO = new GameObject("Background");
                bgGO.transform.SetParent(transform, false);
                backgroundImage = bgGO.AddComponent<Image>();
                backgroundImage.color = normalColor;
                backgroundImage.raycastTarget = false;

                // 设置背景RectTransform
                RectTransform bgRect = bgGO.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
            }

            // 创建物品图标
            if (itemIcon == null)
            {
                GameObject iconGO = new GameObject("ItemIcon");
                iconGO.transform.SetParent(transform, false);
                itemIcon = iconGO.AddComponent<Image>();
                itemIcon.raycastTarget = false;

                // 设置图标RectTransform
                RectTransform iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.1f, 0.1f);
                iconRect.anchorMax = new Vector2(0.9f, 0.9f);
                iconRect.offsetMin = Vector2.zero;
                iconRect.offsetMax = Vector2.zero;
            }

            // 创建数量文本
            if (itemCountText == null)
            {
                GameObject countGO = new GameObject("ItemCount");
                countGO.transform.SetParent(transform, false);
                itemCountText = countGO.AddComponent<TextMeshProUGUI>();
                itemCountText.fontSize = 16;
                itemCountText.alignment = TextAlignmentOptions.BottomRight;
                itemCountText.color = Color.white;
                itemCountText.raycastTarget = false;

                // 设置文本RectTransform
                RectTransform countRect = countGO.GetComponent<RectTransform>();
                countRect.anchorMin = Vector2.zero;
                countRect.anchorMax = Vector2.one;
                countRect.offsetMin = new Vector2(0, 0);
                countRect.offsetMax = new Vector2(0, 0);
            }

            // 创建锁定等级文本
            if (lockLevelText == null)
            {
                GameObject lockGO = new GameObject("LockLevel");
                lockGO.transform.SetParent(transform, false);
                lockLevelText = lockGO.AddComponent<TextMeshProUGUI>();
                lockLevelText.fontSize = 20;
                lockLevelText.alignment = TextAlignmentOptions.Center;
                lockLevelText.color = Color.white;
                lockLevelText.raycastTarget = false;

                // 设置文本RectTransform
                RectTransform lockRect = lockGO.GetComponent<RectTransform>();
                lockRect.anchorMin = Vector2.zero;
                lockRect.anchorMax = Vector2.one;
                lockRect.offsetMin = Vector2.zero;
                lockRect.offsetMax = Vector2.zero;
            }

            // 创建按钮
            if (cellButton == null)
            {
                cellButton = gameObject.AddComponent<Button>();
                cellButton.transition = Selectable.Transition.ColorTint;
                ColorBlock colors = cellButton.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
                colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
                cellButton.colors = colors;
            }
        }

        /// <summary>
        /// 更新格子显示
        /// </summary>
        public void UpdateDisplay(GridCell cell)
        {
            _currentCell = cell;

            if (cell == null)
            {
                SetEmptyDisplay();
                return;
            }

            if (cell.IsLocked)
            {
                SetLockedDisplay(cell);
            }
            else if (cell.HasItem)
            {
                SetFilledDisplay(cell);
            }
            else
            {
                SetEmptyDisplay();
            }
        }

        /// <summary>
        /// 设置空格子显示
        /// </summary>
        private void SetEmptyDisplay()
        {
            if (backgroundImage != null)
                backgroundImage.color = normalColor;

            if (itemIcon != null)
                itemIcon.gameObject.SetActive(false);

            if (itemCountText != null)
                itemCountText.gameObject.SetActive(false);

            if (lockLevelText != null)
                lockLevelText.gameObject.SetActive(false);
        }

        /// <summary>
        /// 设置锁定格子显示
        /// </summary>
        private void SetLockedDisplay(GridCell cell)
        {
            if (backgroundImage != null)
                backgroundImage.color = lockedColor;

            if (itemIcon != null)
                itemIcon.gameObject.SetActive(false);

            if (itemCountText != null)
                itemCountText.gameObject.SetActive(false);

            if (lockLevelText != null)
            {
                lockLevelText.gameObject.SetActive(true);
                lockLevelText.text = $"Lv.{cell.LockedItemLevel}";
            }
        }

        /// <summary>
        /// 设置有物品格子显示
        /// </summary>
        private void SetFilledDisplay(GridCell cell)
        {
            if (backgroundImage != null)
                backgroundImage.color = filledColor;

            if (itemIcon != null)
            {
                itemIcon.gameObject.SetActive(true);
                // TODO: 设置物品图标
                // itemIcon.sprite = GetItemIcon(cell.ItemType);
            }

            if (itemCountText != null)
            {
                itemCountText.gameObject.SetActive(true);
                itemCountText.text = cell.ItemCount.ToString();
            }

            if (lockLevelText != null)
                lockLevelText.gameObject.SetActive(false);
        }

        /// <summary>
        /// 格子点击事件
        /// </summary>
        private void OnCellClicked()
        {
            if (_currentCell != null)
            {
                UIManager.Instance.ShowItemDetail(_currentCell);
            }
        }

        /// <summary>
        /// 获取物品图标
        /// </summary>
        private Sprite GetItemIcon(ItemType itemType)
        {
            return ItemIconManager.Instance.GetItemIcon(itemType);
        }

        /// <summary>
        /// 获取格子位置
        /// </summary>
        public int Row => _row;
        public int Col => _col;
        public GridCell CurrentCell => _currentCell;
    }
}