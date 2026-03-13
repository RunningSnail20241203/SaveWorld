using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestWebGL.Game.Grid;
using TestWebGL.Game.Items;
using TestWebGL.Game.Core;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// 物品详情弹窗 - 点击格子时显示物品详细信息
    /// 显示物品名称、等级、数量、操作按钮等
    /// </summary>
    public class ItemDetailPopup : MonoBehaviour
    {
        [Header("UI组件")]
        public RectTransform popupRect;
        public Image backgroundImage;
        public Image itemIcon;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemLevelText;
        public TextMeshProUGUI itemCountText;
        public TextMeshProUGUI itemDescriptionText;
        public Button closeButton;
        public Button actionButton;

        [Header("面板设置")]
        public Vector2 popupSize = new Vector2(300, 400);
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        private GridCell _currentCell;
        private bool _isVisible = false;

        /// <summary>
        /// 初始化物品详情弹窗
        /// </summary>
        public void Initialize()
        {
            if (popupRect == null)
            {
                CreatePopup();
            }

            CreateUIComponents();
            SetupButtonEvents();

            // 初始隐藏
            Hide();

            Debug.Log("[ItemDetailPopup] 物品详情弹窗初始化完成");
        }

        /// <summary>
        /// 创建弹窗
        /// </summary>
        private void CreatePopup()
        {
            popupRect = GetComponent<RectTransform>();
            if (popupRect == null)
            {
                popupRect = gameObject.AddComponent<RectTransform>();
            }

            // 设置弹窗位置和大小（屏幕中央）
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            popupRect.anchoredPosition = Vector2.zero;
            popupRect.sizeDelta = popupSize;

            // 添加背景
            backgroundImage = gameObject.AddComponent<Image>();
            backgroundImage.color = backgroundColor;

            // 添加垂直布局
            VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
        }

        /// <summary>
        /// 创建UI组件
        /// </summary>
        private void CreateUIComponents()
        {
            // 关闭按钮
            if (closeButton == null)
            {
                GameObject closeGO = new GameObject("CloseButton");
                closeGO.transform.SetParent(transform, false);

                closeButton = closeGO.AddComponent<Button>();
                closeButton.onClick.AddListener(Hide);

                // 设置关闭按钮样式
                Image closeImage = closeGO.AddComponent<Image>();
                closeImage.color = new Color(0.8f, 0.2f, 0.2f);

                // 关闭按钮文本
                GameObject closeTextGO = new GameObject("Text");
                closeTextGO.transform.SetParent(closeGO.transform, false);

                TextMeshProUGUI closeText = closeTextGO.AddComponent<TextMeshProUGUI>();
                closeText.text = "×";
                closeText.fontSize = 24;
                closeText.alignment = TextAlignmentOptions.Center;
                closeText.color = Color.white;

                // 设置关闭按钮位置（右上角）
                RectTransform closeRect = closeGO.GetComponent<RectTransform>();
                closeRect.anchorMin = new Vector2(1f, 1f);
                closeRect.anchorMax = new Vector2(1f, 1f);
                closeRect.pivot = new Vector2(1f, 1f);
                closeRect.anchoredPosition = new Vector2(-10, -10);
                closeRect.sizeDelta = new Vector2(40, 40);

                RectTransform closeTextRect = closeTextGO.GetComponent<RectTransform>();
                closeTextRect.anchorMin = Vector2.zero;
                closeTextRect.anchorMax = Vector2.one;
                closeTextRect.offsetMin = Vector2.zero;
                closeTextRect.offsetMax = Vector2.zero;
            }

            // 物品图标
            if (itemIcon == null)
            {
                GameObject iconGO = new GameObject("ItemIcon");
                iconGO.transform.SetParent(transform, false);

                itemIcon = iconGO.AddComponent<Image>();
                itemIcon.color = Color.white;

                RectTransform iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(80, 80);
            }

            // 物品名称
            if (itemNameText == null)
            {
                GameObject nameGO = new GameObject("ItemName");
                nameGO.transform.SetParent(transform, false);

                itemNameText = nameGO.AddComponent<TextMeshProUGUI>();
                itemNameText.fontSize = 20;
                itemNameText.alignment = TextAlignmentOptions.Center;
                itemNameText.color = Color.white;

                RectTransform nameRect = nameGO.GetComponent<RectTransform>();
                nameRect.sizeDelta = new Vector2(popupSize.x - 40, 30);
            }

            // 物品等级
            if (itemLevelText == null)
            {
                GameObject levelGO = new GameObject("ItemLevel");
                levelGO.transform.SetParent(transform, false);

                itemLevelText = levelGO.AddComponent<TextMeshProUGUI>();
                itemLevelText.fontSize = 16;
                itemLevelText.alignment = TextAlignmentOptions.Center;
                itemLevelText.color = Color.yellow;

                RectTransform levelRect = levelGO.GetComponent<RectTransform>();
                levelRect.sizeDelta = new Vector2(popupSize.x - 40, 25);
            }

            // 物品数量
            if (itemCountText == null)
            {
                GameObject countGO = new GameObject("ItemCount");
                countGO.transform.SetParent(transform, false);

                itemCountText = countGO.AddComponent<TextMeshProUGUI>();
                itemCountText.fontSize = 16;
                itemCountText.alignment = TextAlignmentOptions.Center;
                itemCountText.color = Color.green;

                RectTransform countRect = countGO.GetComponent<RectTransform>();
                countRect.sizeDelta = new Vector2(popupSize.x - 40, 25);
            }

            // 物品描述
            if (itemDescriptionText == null)
            {
                GameObject descGO = new GameObject("ItemDescription");
                descGO.transform.SetParent(transform, false);

                itemDescriptionText = descGO.AddComponent<TextMeshProUGUI>();
                itemDescriptionText.fontSize = 14;
                itemDescriptionText.alignment = TextAlignmentOptions.TopLeft;
                itemDescriptionText.color = Color.gray;
                itemDescriptionText.enableWordWrapping = true;

                RectTransform descRect = descGO.GetComponent<RectTransform>();
                descRect.sizeDelta = new Vector2(popupSize.x - 40, 80);
            }

            // 操作按钮
            if (actionButton == null)
            {
                GameObject actionGO = new GameObject("ActionButton");
                actionGO.transform.SetParent(transform, false);

                actionButton = actionGO.AddComponent<Button>();
                actionButton.onClick.AddListener(OnActionButtonClicked);

                // 设置按钮样式
                Image actionImage = actionGO.AddComponent<Image>();
                actionImage.color = new Color(0.3f, 0.6f, 0.3f);

                // 按钮文本
                GameObject actionTextGO = new GameObject("Text");
                actionTextGO.transform.SetParent(actionGO.transform, false);

                TextMeshProUGUI actionText = actionTextGO.AddComponent<TextMeshProUGUI>();
                actionText.text = "操作";
                actionText.fontSize = 16;
                actionText.alignment = TextAlignmentOptions.Center;
                actionText.color = Color.white;

                // 设置按钮位置和大小
                RectTransform actionRect = actionGO.GetComponent<RectTransform>();
                actionRect.sizeDelta = new Vector2(popupSize.x - 40, 40);

                RectTransform actionTextRect = actionTextGO.GetComponent<RectTransform>();
                actionTextRect.anchorMin = Vector2.zero;
                actionTextRect.anchorMax = Vector2.one;
                actionTextRect.offsetMin = Vector2.zero;
                actionTextRect.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            // 已经在CreateUIComponents中设置了
        }

        /// <summary>
        /// 显示物品详情
        /// </summary>
        public void Show(GridCell cell)
        {
            _currentCell = cell;
            UpdateDisplay();
            gameObject.SetActive(true);
            _isVisible = true;
        }

        /// <summary>
        /// 隐藏物品详情
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _isVisible = false;
            _currentCell = null;
        }

        /// <summary>
        /// 更新显示内容
        /// </summary>
        private void UpdateDisplay()
        {
            if (_currentCell == null) return;

            if (_currentCell.IsLocked)
            {
                // 锁定状态显示
                if (itemNameText != null)
                    itemNameText.text = ItemConfig.GetItemName(_currentCell.LockedItemType);

                if (itemLevelText != null)
                    itemLevelText.text = $"等级: {_currentCell.LockedItemLevel}";

                if (itemCountText != null)
                    itemCountText.text = "状态: 锁定";

                if (itemDescriptionText != null)
                    itemDescriptionText.text = "该格子已被锁定，需要特定的物品才能解锁。";

                if (actionButton != null)
                {
                    TextMeshProUGUI buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = "查看解锁条件";
                }
            }
            else if (_currentCell.HasItem)
            {
                // 有物品状态显示
                if (itemNameText != null)
                    itemNameText.text = ItemConfig.GetItemName(_currentCell.CurrentItemType);

                if (itemLevelText != null)
                    itemLevelText.text = $"等级: {ItemConfig.GetItemLevel(_currentCell.CurrentItemType)}";

                if (itemCountText != null)
                    itemCountText.text = $"数量: {_currentCell.ItemCount}";

                if (itemDescriptionText != null)
                    itemDescriptionText.text = GetItemDescription(_currentCell.CurrentItemType);

                if (actionButton != null)
                {
                    TextMeshProUGUI buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = "合成";
                }
            }
            else
            {
                // 空状态显示
                if (itemNameText != null)
                    itemNameText.text = "空格子";

                if (itemLevelText != null)
                    itemLevelText.text = "等级: -";

                if (itemCountText != null)
                    itemCountText.text = "数量: 0";

                if (itemDescriptionText != null)
                    itemDescriptionText.text = "这是一个空的格子，可以放置物品。";

                if (actionButton != null)
                {
                    TextMeshProUGUI buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                        buttonText.text = "放置物品";
                }
            }

            // 设置物品图标
            if (itemIcon != null)
                itemIcon.sprite = GetItemIcon(_currentCell.IsLocked ? _currentCell.LockedItemType : _currentCell.CurrentItemType);
        }

        /// <summary>
        /// 获取物品描述
        /// </summary>
        private string GetItemDescription(ItemType itemType)
        {
            // 从配置中获取物品描述
            return ItemConfig.GetItemDescription(itemType);
        }

        /// <summary>
        /// 操作按钮点击事件
        /// </summary>
        private void OnActionButtonClicked()
        {
            if (_currentCell == null) return;

            if (_currentCell.IsLocked)
            {
                // 显示解锁条件
                Debug.Log($"查看解锁条件: 需要 {ItemConfig.GetItemName(_currentCell.LockedItemType)} 等级 {_currentCell.LockedItemLevel}");
            }
            else if (_currentCell.HasItem)
            {
                // 执行合成
                var craftingEngine = GameManager.Instance.GetCraftingEngine();
                craftingEngine.TryDoubleTapCraft(_currentCell.row, _currentCell.column);
                Hide(); // 关闭弹窗
            }
            else
            {
                // 放置物品（暂时关闭弹窗）
                Debug.Log("打开物品选择界面");
                Hide();
            }
        }

        /// <summary>
        /// 获取物品图标
        /// </summary>
        private Sprite GetItemIcon(ItemType itemType)
        {
            return ItemIconManager.Instance.GetItemIcon(itemType);
        }

        public bool IsVisible => _isVisible;
        public GridCell CurrentCell => _currentCell;
    }
}