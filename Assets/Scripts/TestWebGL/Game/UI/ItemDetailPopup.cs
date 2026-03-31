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
            // 从预制件获取RectTransform
            if (popupRect == null)
            {
                popupRect = GetComponent<RectTransform>();
            }

            // 从预制件获取UI组件引用
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            if (itemIcon == null)
                itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
            if (itemNameText == null)
                itemNameText = transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
            if (itemLevelText == null)
                itemLevelText = transform.Find("ItemLevel")?.GetComponent<TextMeshProUGUI>();
            if (itemCountText == null)
                itemCountText = transform.Find("ItemCount")?.GetComponent<TextMeshProUGUI>();
            if (itemDescriptionText == null)
                itemDescriptionText = transform.Find("ItemDescription")?.GetComponent<TextMeshProUGUI>();
            if (closeButton == null)
                closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
            if (actionButton == null)
                actionButton = transform.Find("ActionButton")?.GetComponent<Button>();

            // 设置按钮事件
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionButtonClicked);

            // 初始隐藏
            Hide();

            Debug.Log("[ItemDetailPopup] 物品详情弹窗初始化完成");
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