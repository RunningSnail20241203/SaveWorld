using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TestWebGL.Game.Order;
using TestWebGL.Game.Core;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// 订单面板 - 显示当前订单和订单历史
    /// 包括订单列表、订单详情、完成状态等
    /// </summary>
    public class OrdersPanel : MonoBehaviour
    {
        [Header("UI组件")]
        public RectTransform panelRect;
        public Image backgroundImage;
        public TextMeshProUGUI titleText;
        public Button closeButton;

        [Header("订单列表")]
        public ScrollRect ordersScrollRect;
        public RectTransform ordersContent;
        public GameObject orderItemPrefab;

        [Header("订单详情")]
        public TextMeshProUGUI orderTitleText;
        public TextMeshProUGUI orderDescriptionText;
        public TextMeshProUGUI orderRewardText;
        public TextMeshProUGUI orderTimeText;
        public TextMeshProUGUI orderStatusText;
        public Button completeOrderButton;

        [Header("标签页")]
        public Button activeOrdersTab;
        public Button completedOrdersTab;
        public Image activeTabIndicator;
        public Image completedTabIndicator;

        [Header("面板设置")]
        public Vector2 panelSize = new Vector2(500, 600);
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        private bool _isVisible = false;
        private List<OrderItemUI> _orderItems = new List<OrderItemUI>();
        private OrderSystem.OrderData _selectedOrder;
        private bool _showingActiveOrders = true;

        /// <summary>
        /// 初始化订单面板
        /// </summary>
        public void Initialize()
        {
            if (panelRect == null)
            {
                CreatePanel();
            }

            CreateUIComponents();
            SetupButtonEvents();

            // 初始隐藏
            Hide();

            Debug.Log("[OrdersPanel] 订单面板初始化完成");
        }

        /// <summary>
        /// 创建面板
        /// </summary>
        private void CreatePanel()
        {
            panelRect = GetComponent<RectTransform>();
            if (panelRect == null)
            {
                panelRect = gameObject.AddComponent<RectTransform>();
            }

            // 设置面板位置和大小（屏幕中央）
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = panelSize;

            // 添加背景 - 先检查是否已存在Image组件
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
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
            // 标题
            if (titleText == null)
            {
                GameObject titleGO = new GameObject("Title");
                titleGO.transform.SetParent(transform, false);

                titleText = titleGO.AddComponent<TextMeshProUGUI>();
                titleText.text = "订单管理";
                titleText.fontSize = 24;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = Color.white;

                RectTransform titleRect = titleGO.GetComponent<RectTransform>();
                titleRect.sizeDelta = new Vector2(panelSize.x - 40, 40);
            }

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

            // 标签页
            CreateTabs();

            // 订单列表区域
            CreateOrdersList();

            // 订单详情区域
            CreateOrderDetails();
        }

        /// <summary>
        /// 创建标签页
        /// </summary>
        private void CreateTabs()
        {
            // 标签页容器
            GameObject tabsContainer = new GameObject("TabsContainer");
            tabsContainer.transform.SetParent(transform, false);

            HorizontalLayoutGroup tabsLayout = tabsContainer.AddComponent<HorizontalLayoutGroup>();
            tabsLayout.spacing = 10;
            tabsLayout.childAlignment = TextAnchor.MiddleCenter;

            RectTransform tabsRect = tabsContainer.AddComponent<RectTransform>();
            tabsRect.sizeDelta = new Vector2(panelSize.x - 40, 50);

            // 活跃订单标签
            if (activeOrdersTab == null)
            {
                activeOrdersTab = CreateTab(tabsContainer, "ActiveOrdersTab", "活跃订单", OnActiveOrdersTabClicked);
            }

            // 已完成订单标签
            if (completedOrdersTab == null)
            {
                completedOrdersTab = CreateTab(tabsContainer, "CompletedOrdersTab", "已完成", OnCompletedOrdersTabClicked);
            }

            // 标签指示器
            if (activeTabIndicator == null)
            {
                GameObject indicatorGO = new GameObject("ActiveTabIndicator");
                indicatorGO.transform.SetParent(activeOrdersTab.transform, false);

                activeTabIndicator = indicatorGO.AddComponent<Image>();
                activeTabIndicator.color = Color.green;

                RectTransform indicatorRect = indicatorGO.GetComponent<RectTransform>();
                indicatorRect.anchorMin = new Vector2(0f, 0f);
                indicatorRect.anchorMax = new Vector2(1f, 0f);
                indicatorRect.pivot = new Vector2(0.5f, 0f);
                indicatorRect.anchoredPosition = Vector2.zero;
                indicatorRect.sizeDelta = new Vector2(0, 3);
            }

            if (completedTabIndicator == null)
            {
                GameObject indicatorGO = new GameObject("CompletedTabIndicator");
                indicatorGO.transform.SetParent(completedOrdersTab.transform, false);

                completedTabIndicator = indicatorGO.AddComponent<Image>();
                completedTabIndicator.color = Color.green;

                RectTransform indicatorRect = indicatorGO.GetComponent<RectTransform>();
                indicatorRect.anchorMin = new Vector2(0f, 0f);
                indicatorRect.anchorMax = new Vector2(1f, 0f);
                indicatorRect.pivot = new Vector2(0.5f, 0f);
                indicatorRect.anchoredPosition = Vector2.zero;
                indicatorRect.sizeDelta = new Vector2(0, 3);
            }

            // 初始状态
            UpdateTabIndicators();
        }

        /// <summary>
        /// 创建标签
        /// </summary>
        private Button CreateTab(GameObject parent, string name, string text, UnityEngine.Events.UnityAction action)
        {
            GameObject tabGO = new GameObject(name);
            tabGO.transform.SetParent(parent.transform, false);

            Button button = tabGO.AddComponent<Button>();
            button.onClick.AddListener(action);

            // 设置标签样式
            Image tabImage = tabGO.AddComponent<Image>();
            tabImage.color = new Color(0.2f, 0.2f, 0.2f);

            // 标签文本
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(tabGO.transform, false);

            TextMeshProUGUI tabText = textGO.AddComponent<TextMeshProUGUI>();
            tabText.text = text;
            tabText.fontSize = 16;
            tabText.alignment = TextAlignmentOptions.Center;
            tabText.color = Color.white;

            // 设置标签大小
            RectTransform tabRect = tabGO.AddComponent<RectTransform>();
            tabRect.sizeDelta = new Vector2(120, 40);

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        /// <summary>
        /// 创建订单列表
        /// </summary>
        private void CreateOrdersList()
        {
            // 列表容器
            GameObject listContainer = new GameObject("OrdersList");
            listContainer.transform.SetParent(transform, false);

            RectTransform listRect = listContainer.AddComponent<RectTransform>();
            listRect.sizeDelta = new Vector2(panelSize.x - 40, 200);

            // 滚动视图
            GameObject scrollGO = new GameObject("ScrollView");
            scrollGO.transform.SetParent(listContainer.transform, false);

            ordersScrollRect = scrollGO.AddComponent<ScrollRect>();

            RectTransform scrollRect = scrollGO.GetComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;

            // 视口
            GameObject viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);

            Image viewportImage = viewportGO.AddComponent<Image>();
            viewportImage.color = new Color(0.15f, 0.15f, 0.15f);

            RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            // 内容区域
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);

            ordersContent = contentGO.GetComponent<RectTransform>();
            ordersContent.anchorMin = new Vector2(0f, 1f);
            ordersContent.anchorMax = new Vector2(1f, 1f);
            ordersContent.pivot = new Vector2(0f, 1f);
            ordersContent.anchoredPosition = Vector2.zero;
            ordersContent.sizeDelta = new Vector2(0, 200);

            VerticalLayoutGroup contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 5;
            contentLayout.childAlignment = TextAnchor.UpperCenter;

            // 设置滚动视图引用
            ordersScrollRect.viewport = viewportRect;
            ordersScrollRect.content = ordersContent;
        }

        /// <summary>
        /// 创建订单详情
        /// </summary>
        private void CreateOrderDetails()
        {
            // 详情容器
            GameObject detailsContainer = new GameObject("OrderDetails");
            detailsContainer.transform.SetParent(transform, false);

            VerticalLayoutGroup detailsLayout = detailsContainer.AddComponent<VerticalLayoutGroup>();
            detailsLayout.padding = new RectOffset(10, 10, 10, 10);
            detailsLayout.spacing = 8;
            detailsLayout.childAlignment = TextAnchor.UpperLeft;

            RectTransform detailsRect = detailsContainer.AddComponent<RectTransform>();
            detailsRect.sizeDelta = new Vector2(panelSize.x - 40, 150);

            // 背景
            Image detailsBG = detailsContainer.AddComponent<Image>();
            detailsBG.color = new Color(0.15f, 0.15f, 0.15f);

            // 订单标题
            if (orderTitleText == null)
            {
                GameObject titleGO = new GameObject("OrderTitle");
                titleGO.transform.SetParent(detailsContainer.transform, false);

                orderTitleText = titleGO.AddComponent<TextMeshProUGUI>();
                orderTitleText.text = "选择订单查看详情";
                orderTitleText.fontSize = 16;
                orderTitleText.color = Color.white;
                orderTitleText.fontStyle = FontStyles.Bold;

                RectTransform titleRect = titleGO.GetComponent<RectTransform>();
                titleRect.sizeDelta = new Vector2(panelSize.x - 60, 25);
            }

            // 订单描述
            if (orderDescriptionText == null)
            {
                GameObject descGO = new GameObject("OrderDescription");
                descGO.transform.SetParent(detailsContainer.transform, false);

                orderDescriptionText = descGO.AddComponent<TextMeshProUGUI>();
                orderDescriptionText.text = "";
                orderDescriptionText.fontSize = 14;
                orderDescriptionText.color = Color.gray;
                orderDescriptionText.enableWordWrapping = true;

                RectTransform descRect = descGO.GetComponent<RectTransform>();
                descRect.sizeDelta = new Vector2(panelSize.x - 60, 40);
            }

            // 订单奖励
            if (orderRewardText == null)
            {
                GameObject rewardGO = new GameObject("OrderReward");
                rewardGO.transform.SetParent(detailsContainer.transform, false);

                orderRewardText = rewardGO.AddComponent<TextMeshProUGUI>();
                orderRewardText.text = "";
                orderRewardText.fontSize = 14;
                orderRewardText.color = Color.yellow;

                RectTransform rewardRect = rewardGO.GetComponent<RectTransform>();
                rewardRect.sizeDelta = new Vector2(panelSize.x - 60, 25);
            }

            // 订单时间和状态
            GameObject timeStatusContainer = new GameObject("TimeStatusContainer");
            timeStatusContainer.transform.SetParent(detailsContainer.transform, false);

            HorizontalLayoutGroup timeStatusLayout = timeStatusContainer.AddComponent<HorizontalLayoutGroup>();
            timeStatusLayout.spacing = 20;
            timeStatusLayout.childAlignment = TextAnchor.MiddleLeft;

            RectTransform timeStatusRect = timeStatusContainer.GetComponent<RectTransform>();
            timeStatusRect.sizeDelta = new Vector2(panelSize.x - 60, 25);

            // 时间
            if (orderTimeText == null)
            {
                GameObject timeGO = new GameObject("OrderTime");
                timeGO.transform.SetParent(timeStatusContainer.transform, false);

                orderTimeText = timeGO.AddComponent<TextMeshProUGUI>();
                orderTimeText.text = "";
                orderTimeText.fontSize = 12;
                orderTimeText.color = Color.cyan;

                RectTransform timeRect = timeGO.GetComponent<RectTransform>();
                timeRect.sizeDelta = new Vector2(150, 25);
            }

            // 状态
            if (orderStatusText == null)
            {
                GameObject statusGO = new GameObject("OrderStatus");
                statusGO.transform.SetParent(timeStatusContainer.transform, false);

                orderStatusText = statusGO.AddComponent<TextMeshProUGUI>();
                orderStatusText.text = "";
                orderStatusText.fontSize = 12;
                orderStatusText.color = Color.green;

                RectTransform statusRect = statusGO.GetComponent<RectTransform>();
                statusRect.sizeDelta = new Vector2(100, 25);
            }

            // 完成按钮
            if (completeOrderButton == null)
            {
                GameObject buttonGO = new GameObject("CompleteOrderButton");
                buttonGO.transform.SetParent(detailsContainer.transform, false);

                completeOrderButton = buttonGO.AddComponent<Button>();
                completeOrderButton.onClick.AddListener(OnCompleteOrderButtonClicked);

                // 设置按钮样式
                Image buttonImage = buttonGO.AddComponent<Image>();
                buttonImage.color = new Color(0.3f, 0.6f, 0.3f);

                // 按钮文本
                GameObject textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform, false);

                TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
                buttonText.text = "完成订单";
                buttonText.fontSize = 14;
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.color = Color.white;

                // 设置按钮大小
                RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
                buttonRect.sizeDelta = new Vector2(120, 30);

                RectTransform textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            // 已在创建时设置
        }

        /// <summary>
        /// 显示订单面板
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            _isVisible = true;
            RefreshOrdersList();
        }

        /// <summary>
        /// 隐藏订单面板
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _isVisible = false;
        }

        /// <summary>
        /// 刷新订单显示
        /// </summary>
        public void Refresh()
        {
            if (_isVisible)
            {
                RefreshOrdersList();
            }
        }

        /// <summary>
        /// 活跃订单标签点击事件
        /// </summary>
        private void OnActiveOrdersTabClicked()
        {
            _showingActiveOrders = true;
            UpdateTabIndicators();
            RefreshOrdersList();
        }

        /// <summary>
        /// 已完成订单标签点击事件
        /// </summary>
        private void OnCompletedOrdersTabClicked()
        {
            _showingActiveOrders = false;
            UpdateTabIndicators();
            RefreshOrdersList();
        }

        /// <summary>
        /// 更新标签指示器
        /// </summary>
        private void UpdateTabIndicators()
        {
            if (activeTabIndicator != null)
                activeTabIndicator.gameObject.SetActive(_showingActiveOrders);
            if (completedTabIndicator != null)
                completedTabIndicator.gameObject.SetActive(!_showingActiveOrders);
        }

        /// <summary>
        /// 刷新订单列表
        /// </summary>
        private void RefreshOrdersList()
        {
            // 清除现有项目
            foreach (var item in _orderItems)
            {
                if (item != null && item.gameObject != null)
                    Destroy(item.gameObject);
            }
            _orderItems.Clear();

            // 获取订单数据 - 使用OrderSystem
            var orderSystem = GameManager.Instance.GetOrderSystem();
            if (orderSystem == null) return;

            // 获取当前订单
            var orders = orderSystem.GetCurrentOrders();
            if (orders == null || orders.Length == 0) return;

            // 过滤订单（活跃 vs 已完成）
            List<OrderSystem.OrderData> filteredOrders = new List<OrderSystem.OrderData>();
            foreach (var order in orders)
            {
                if (_showingActiveOrders && !order.isCompleted)
                {
                    filteredOrders.Add(order);
                }
                else if (!_showingActiveOrders && order.isCompleted)
                {
                    filteredOrders.Add(order);
                }
            }

            // 创建订单项目
            foreach (var order in filteredOrders)
            {
                CreateOrderItem(order);
            }

            // 清除选择
            _selectedOrder = default;
            UpdateOrderDetails();
        }

        /// <summary>
        /// 创建订单项目
        /// </summary>
        private void CreateOrderItem(OrderSystem.OrderData order)
        {
            if (ordersContent == null) return;

            GameObject itemGO = new GameObject("OrderItem");
            itemGO.transform.SetParent(ordersContent, false);

            OrderItemUI itemUI = itemGO.AddComponent<OrderItemUI>();
            itemUI.Initialize(order, OnOrderItemClicked);

            _orderItems.Add(itemUI);

            RectTransform itemRect = itemGO.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(panelSize.x - 60, 50);
        }

        /// <summary>
        /// 订单项目点击事件
        /// </summary>
        private void OnOrderItemClicked(OrderSystem.OrderData order)
        {
            _selectedOrder = order;
            UpdateOrderDetails();
        }

        /// <summary>
        /// 更新订单详情
        /// </summary>
        private void UpdateOrderDetails()
        {
            if (_selectedOrder.Equals(default(OrderSystem.OrderData)))
            {
                if (orderTitleText != null)
                    orderTitleText.text = "选择订单查看详情";
                if (orderDescriptionText != null)
                    orderDescriptionText.text = "";
                if (orderRewardText != null)
                    orderRewardText.text = "";
                if (orderTimeText != null)
                    orderTimeText.text = "";
                if (orderStatusText != null)
                    orderStatusText.text = "";
                if (completeOrderButton != null)
                    completeOrderButton.gameObject.SetActive(false);
                return;
            }

            if (orderTitleText != null)
                orderTitleText.text = _selectedOrder.GetDisplayName();

            if (orderDescriptionText != null)
                orderDescriptionText.text = $"需要: {_selectedOrder.GetDisplayName()}";

            if (orderRewardText != null)
                orderRewardText.text = $"奖励: {_selectedOrder.rewardType} ×{_selectedOrder.rewardAmount}";

            if (orderTimeText != null)
                orderTimeText.text = $"创建时间: {_selectedOrder.createdTime.ToString("yyyy-MM-dd HH:mm")}";

            if (orderStatusText != null)
            {
                string statusText = _selectedOrder.isCompleted ? "已完成" : "进行中";
                Color statusColor = _selectedOrder.isCompleted ? Color.green : Color.yellow;
                orderStatusText.text = statusText;
                orderStatusText.color = statusColor;
            }

            if (completeOrderButton != null)
            {
                completeOrderButton.gameObject.SetActive(_selectedOrder.isCompleted);
                TextMeshProUGUI buttonText = completeOrderButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = "领取奖励";
            }
        }

        /// <summary>
        /// 完成订单按钮点击事件
        /// </summary>
        private void OnCompleteOrderButtonClicked()
        {
            if (_selectedOrder.Equals(default(OrderSystem.OrderData)) || !_selectedOrder.isCompleted) return;

            var orderEngine = GameManager.Instance.GetOrderEngine();
            if (orderEngine != null)
            {
                // 尝试提交订单（这里简化处理，实际应该检查玩家是否满足订单要求）
                orderEngine.TrySubmitOrder(_selectedOrder.orderId);
                RefreshOrdersList();
                UpdateOrderDetails();
                Debug.Log($"[OrdersPanel] 提交订单: {_selectedOrder.GetDisplayName()}");
            }
        }

        public bool IsVisible => _isVisible;
    }

    /// <summary>
    /// 订单项目UI组件
    /// </summary>
    public class OrderItemUI : MonoBehaviour
    {
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI statusText;
        public Button button;
        public Image background;

        private OrderSystem.OrderData _orderData;

        public void Initialize(OrderSystem.OrderData order, System.Action<OrderSystem.OrderData> onClick)
        {
            _orderData = order;

            CreateUI();
            UpdateDisplay();

            if (button != null)
                button.onClick.AddListener(() => onClick?.Invoke(_orderData));
        }

        private void CreateUI()
        {
            // 背景
            if (background == null)
            {
                background = gameObject.AddComponent<Image>();
                background.color = new Color(0.2f, 0.2f, 0.2f);
            }

            // 按钮
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            // 水平布局
            HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;

            // 标题文本
            if (titleText == null)
            {
                GameObject titleGO = new GameObject("Title");
                titleGO.transform.SetParent(transform, false);

                titleText = titleGO.AddComponent<TextMeshProUGUI>();
                titleText.fontSize = 14;
                titleText.color = Color.white;

                RectTransform titleRect = titleGO.AddComponent<RectTransform>();
                titleRect.sizeDelta = new Vector2(200, 40);
            }

            // 状态文本
            if (statusText == null)
            {
                GameObject statusGO = new GameObject("Status");
                statusGO.transform.SetParent(transform, false);

                statusText = statusGO.AddComponent<TextMeshProUGUI>();
                statusText.fontSize = 12;
                statusText.alignment = TextAlignmentOptions.Right;
                statusText.color = Color.yellow;

                RectTransform statusRect = statusGO.AddComponent<RectTransform>();
                statusRect.sizeDelta = new Vector2(100, 40);
            }
        }

        private void UpdateDisplay()
        {
            if (_orderData.Equals(default(OrderSystem.OrderData))) return;

            if (titleText != null)
                titleText.text = _orderData.GetDisplayName();

            if (statusText != null)
            {
                if (_orderData.isCompleted)
                {
                    statusText.text = "已完成";
                    statusText.color = Color.green;
                }
                else
                {
                    statusText.text = "进行中";
                    statusText.color = Color.yellow;
                }
            }

            if (background != null)
            {
                background.color = _orderData.isCompleted ?
                    new Color(0.3f, 0.6f, 0.3f) : new Color(0.2f, 0.2f, 0.2f);
            }
        }
    }
}