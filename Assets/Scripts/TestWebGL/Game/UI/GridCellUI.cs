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
        public Color normalColor = UIThemeConfig.BorderNormal;
        public Color lockedColor = UIThemeConfig.BackgroundCard;
        public Color filledColor = UIThemeConfig.BorderHighlight;
        public Color highlightColor = UIThemeConfig.BorderHighlight;

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

            // 从预制件加载背景图片
            if (backgroundImage == null)
            {
                GameObject bgPrefab = Resources.Load<GameObject>("Prefabs/UI/GridCellBackground");
                if (bgPrefab != null)
                {
                    GameObject bgGO = Instantiate(bgPrefab, transform);
                    bgGO.name = "Background";
                    backgroundImage = bgGO.GetComponent<Image>();
                }
                else
                {
                    Debug.LogError("[GridCellUI] 无法加载GridCellBackground预制件");
                }
            }

            // 从预制件加载物品图标
            if (itemIcon == null)
            {
                GameObject iconPrefab = Resources.Load<GameObject>("Prefabs/UI/GridCellItemIcon");
                if (iconPrefab != null)
                {
                    GameObject iconGO = Instantiate(iconPrefab, transform);
                    iconGO.name = "ItemIcon";
                    itemIcon = iconGO.GetComponent<Image>();
                }
                else
                {
                    Debug.LogError("[GridCellUI] 无法加载GridCellItemIcon预制件");
                }
            }

            // 从预制件加载数量文本
            if (itemCountText == null)
            {
                GameObject countPrefab = Resources.Load<GameObject>("Prefabs/UI/GridCellItemCount");
                if (countPrefab != null)
                {
                    GameObject countGO = Instantiate(countPrefab, transform);
                    countGO.name = "ItemCount";
                    itemCountText = countGO.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    Debug.LogError("[GridCellUI] 无法加载GridCellItemCount预制件");
                }
            }

            // 从预制件加载锁定等级文本
            if (lockLevelText == null)
            {
                GameObject lockPrefab = Resources.Load<GameObject>("Prefabs/UI/GridCellLockLevel");
                if (lockPrefab != null)
                {
                    GameObject lockGO = Instantiate(lockPrefab, transform);
                    lockGO.name = "LockLevel";
                    lockLevelText = lockGO.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    Debug.LogError("[GridCellUI] 无法加载GridCellLockLevel预制件");
                }
            }

            // 创建按钮
            if (cellButton == null)
            {
                cellButton = gameObject.AddComponent<Button>();
                cellButton.transition = Selectable.Transition.ColorTint;
                ColorBlock colors = cellButton.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
                colors.pressedColor = new Color(1f, 1f, 1f, 0.7f);
                colors.fadeDuration = 0.1f;
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
            {
                backgroundImage.color = normalColor;
                backgroundImage.sprite = UIThemeConfig.CreateSolidSprite(UIThemeConfig.BackgroundCard);
            }

            if (itemIcon != null)
                itemIcon.gameObject.SetActive(false);

            if (itemCountText != null)
            {
                itemCountText.gameObject.SetActive(false);
                itemCountText.color = UIThemeConfig.TextPrimary;
                itemCountText.fontSize = UIThemeConfig.FontSizeNumber;
            }

            if (lockLevelText != null)
            {
                lockLevelText.gameObject.SetActive(false);
                lockLevelText.color = UIThemeConfig.TextSecondary;
                lockLevelText.fontSize = UIThemeConfig.FontSizeSmall;
            }
        }

        /// <summary>
        /// 设置锁定格子显示
        /// </summary>
        private void SetLockedDisplay(GridCell cell)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = lockedColor;
                backgroundImage.sprite = UIThemeConfig.CreateSolidSprite(UIThemeConfig.LockedOverlay);
            }

            if (itemIcon != null)
                itemIcon.gameObject.SetActive(false);

            if (itemCountText != null)
                itemCountText.gameObject.SetActive(false);

            if (lockLevelText != null)
            {
                lockLevelText.gameObject.SetActive(true);
                lockLevelText.text = $"锁定Lv.{cell.LockedItemLevel}";
                lockLevelText.color = UIThemeConfig.TextSecondary;
                lockLevelText.fontSize = UIThemeConfig.FontSizeSmall;
            }
        }

        /// <summary>
        /// 设置有物品格子显示
        /// </summary>
        private void SetFilledDisplay(GridCell cell)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = filledColor;
                backgroundImage.sprite = UIThemeConfig.CreateSolidSprite(UIThemeConfig.BackgroundCard);
            }

            if (itemIcon != null)
            {
                itemIcon.gameObject.SetActive(true);
                itemIcon.sprite = GetItemIcon(cell.CurrentItemType);
            }

            if (itemCountText != null)
            {
                itemCountText.gameObject.SetActive(true);
                itemCountText.text = cell.ItemCount.ToString();
                itemCountText.color = UIThemeConfig.TextPrimary;
                itemCountText.fontSize = UIThemeConfig.FontSizeNumber;
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