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
        
        [Header("预制件")]
        public GameObject orderItemPrefabAsset;

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
            // 从预制件加载
            if (panelRect == null)
            {
                LoadFromPrefab();
            }

            SetupButtonEvents();

            // 初始隐藏
            Hide();

            Debug.Log("[OrdersPanel] 订单面板初始化完成");
        }

        /// <summary>
        /// 从预制件加载
        /// </summary>
        private void LoadFromPrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/UI/OrdersPanel");
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.name = "OrdersPanel";
                
                // 获取组件引用
                panelRect = instance.GetComponent<RectTransform>();
                backgroundImage = instance.GetComponent<Image>();
                titleText = instance.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                closeButton = instance.transform.Find("CloseButton")?.GetComponent<Button>();
                
                // 获取标签页
                Transform tabsContainer = instance.transform.Find("TabsContainer");
                if (tabsContainer != null)
                {
                    activeOrdersTab = tabsContainer.Find("ActiveOrdersTab")?.GetComponent<Button>();
                    completedOrdersTab = tabsContainer.Find("CompletedOrdersTab")?.GetComponent<Button>();
                    
                    if (activeOrdersTab != null)
                        activeTabIndicator = activeOrdersTab.transform.Find("ActiveTabIndicator")?.GetComponent<Image>();
                    if (completedOrdersTab != null)
                        completedTabIndicator = completedOrdersTab.transform.Find("CompletedTabIndicator")?.GetComponent<Image>();
                }
                
                // 获取订单列表
                Transform ordersList = instance.transform.Find("OrdersList");
                if (ordersList != null)
                {
                    Transform scrollView = ordersList.Find("ScrollView");
                    if (scrollView != null)
                    {
                        ordersScrollRect = scrollView.GetComponent<ScrollRect>();
                        Transform viewport = scrollView.Find("Viewport");
                        if (viewport != null)
                        {
                            Transform content = viewport.Find("Content");
                            if (content != null)
                            {
                                ordersContent = content.GetComponent<RectTransform>();
                            }
                        }
                    }
                }
                
                // 获取订单详情
                Transform orderDetails = instance.transform.Find("OrderDetails");
                if (orderDetails != null)
                {
                    orderTitleText = orderDetails.Find("OrderTitle")?.GetComponent<TextMeshProUGUI>();
                    orderDescriptionText = orderDetails.Find("OrderDescription")?.GetComponent<TextMeshProUGUI>();
                    orderRewardText = orderDetails.Find("OrderReward")?.GetComponent<TextMeshProUGUI>();
                    
                    Transform timeStatusContainer = orderDetails.Find("TimeStatusContainer");
                    if (timeStatusContainer != null)
                    {
                        orderTimeText = timeStatusContainer.Find("OrderTime")?.GetComponent<TextMeshProUGUI>();
                        orderStatusText = timeStatusContainer.Find("OrderStatus")?.GetComponent<TextMeshProUGUI>();
                    }
                    
                    completeOrderButton = orderDetails.Find("CompleteOrderButton")?.GetComponent<Button>();
                }
                
                // 加载OrderItem预制件
                orderItemPrefabAsset = Resources.Load<GameObject>("Prefabs/UI/OrderItem");
                if (orderItemPrefabAsset == null)
                {
                    Debug.LogWarning("[OrdersPanel] 无法加载OrderItem预制件");
                }
            }
            else
            {
                Debug.LogError("[OrdersPanel] 无法加载OrdersPanel预制件");
            }
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            // 设置关闭按钮事件
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
            
            // 设置标签页按钮事件
            if (activeOrdersTab != null)
            {
                activeOrdersTab.onClick.AddListener(OnActiveOrdersTabClicked);
            }
            
            if (completedOrdersTab != null)
            {
                completedOrdersTab.onClick.AddListener(OnCompletedOrdersTabClicked);
            }
            
            // 设置完成订单按钮事件
            if (completeOrderButton != null)
            {
                completeOrderButton.onClick.AddListener(OnCompleteOrderButtonClicked);
            }
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
            if (orderItemPrefabAsset == null)
            {
                Debug.LogError("[OrdersPanel] OrderItem预制件未设置");
                return;
            }

            GameObject itemGO = Instantiate(orderItemPrefabAsset, ordersContent);
            itemGO.name = "OrderItem";

            OrderItemUI itemUI = itemGO.GetComponent<OrderItemUI>();
            if (itemUI == null)
            {
                itemUI = itemGO.AddComponent<OrderItemUI>();
            }
            itemUI.Initialize(order, OnOrderItemClicked);

            _orderItems.Add(itemUI);

            RectTransform itemRect = itemGO.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.sizeDelta = new Vector2(panelSize.x - 60, 50);
            }
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
            // 从预制件加载子组件
            if (background == null)
            {
                GameObject bgPrefab = Resources.Load<GameObject>("Prefabs/UI/OrderItemBackground");
                if (bgPrefab != null)
                {
                    GameObject bgGO = Instantiate(bgPrefab, transform);
                    bgGO.name = "Background";
                    background = bgGO.GetComponent<Image>();
                }
                else
                {
                    Debug.LogError("[OrderItemUI] 无法加载OrderItemBackground预制件");
                }
            }

            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            // 水平布局
            HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;

            if (titleText == null)
            {
                GameObject titlePrefab = Resources.Load<GameObject>("Prefabs/UI/OrderItemTitle");
                if (titlePrefab != null)
                {
                    GameObject titleGO = Instantiate(titlePrefab, transform);
                    titleGO.name = "Title";
                    titleText = titleGO.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    Debug.LogError("[OrderItemUI] 无法加载OrderItemTitle预制件");
                }
            }

            if (statusText == null)
            {
                GameObject statusPrefab = Resources.Load<GameObject>("Prefabs/UI/OrderItemStatus");
                if (statusPrefab != null)
                {
                    GameObject statusGO = Instantiate(statusPrefab, transform);
                    statusGO.name = "Status";
                    statusText = statusGO.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    Debug.LogError("[OrderItemUI] 无法加载OrderItemStatus预制件");
                }
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