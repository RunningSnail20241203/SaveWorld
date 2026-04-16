using UnityEngine;
using UnityEngine.UI;
using SaveWorld.Game.Order;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 订单UI
    /// </summary>
    public class OrderUI : UIPanelBase
    {
        [Header("订单列表")]
        public Transform OrderContainer;
        public GameObject OrderItemPrefab;

        private OrderItemUI[] _orderItems;

        public override void Initialize()
        {
            base.Initialize();

            // 创建3个订单槽位
            _orderItems = new OrderItemUI[3];
            for (int i = 0; i < 3; i++)
            {
                var obj = Instantiate(OrderItemPrefab, OrderContainer);
                _orderItems[i] = obj.GetComponent<OrderItemUI>();
            }

            RefreshOrders();

            // 监听订单更新
            OrderManager.Instance.OnOrdersUpdated += RefreshOrders;
        }

        public override void Refresh()
        {
            RefreshOrders();
        }

        /// <summary>
        /// 刷新所有订单
        /// </summary>
        public void RefreshOrders()
        {
            var orders = OrderManager.Instance.GetActiveOrders();

            for (int i = 0; i < 3; i++)
            {
                if (i < orders.Count)
                {
                    _orderItems[i].UpdateOrder(orders[i]);
                    _orderItems[i].gameObject.SetActive(true);
                }
                else
                {
                    _orderItems[i].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 订单项UI
    /// </summary>
    public class OrderItemUI : MonoBehaviour
    {
        public Image RequireItemIcon;
        public Text RewardExpText;
        public Text RewardGoldText;
        public Text TimeLeftText;
        public Button SubmitButton;
        public Button RefreshButton;

        private OrderData _currentOrder;

        public void UpdateOrder(OrderData order)
        {
            _currentOrder = order;

            RequireItemIcon.sprite = Items.ItemIconManager.Instance.GetIcon(order.RequireItem);
            RewardExpText.text = order.RewardExp.ToString();
            RewardGoldText.text = order.RewardGold.ToString();

            TimeSpan remaining = order.ExpireTime - DateTime.Now;
            TimeLeftText.text = $"{remaining.Hours:D2}:{remaining.Minutes:D2}";

            SubmitButton.interactable = !order.IsCompleted && Grid.GridManager.Instance.HasItem(order.RequireItem);
            SubmitButton.onClick.RemoveAllListeners();
            SubmitButton.onClick.AddListener(OnSubmitClicked);

            RefreshButton.onClick.RemoveAllListeners();
            RefreshButton.onClick.AddListener(OnRefreshClicked);
        }

        private void OnSubmitClicked()
        {
            if (_currentOrder != null)
            {
                OrderManager.Instance.TrySubmitOrder(_currentOrder.OrderId);
            }
        }

        private void OnRefreshClicked()
        {
            if (_currentOrder != null)
            {
                OrderManager.Instance.RefreshOrder(_currentOrder.OrderId);
            }
        }
    }
}
