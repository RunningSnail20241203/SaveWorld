using System;
using UnityEngine;
using UnityEngine.UI;
using SaveWorld.Game.Order;
using SaveWorld.Game.Core;

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
        private bool _initialized = false;

        void Awake()
        {
            var panel = GetComponent<OrdersPanel>();
            if (panel != null)
            {
                if (OrderContainer == null) OrderContainer = panel.ordersContent;
            }
        }

        public override void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // 创建3个订单槽位
            _orderItems = new OrderItemUI[3];
            for (int i = 0; i < 3; i++)
            {
                if (OrderItemPrefab != null && OrderContainer != null)
                {
                    var obj = UnityEngine.Object.Instantiate(OrderItemPrefab, OrderContainer);
                    _orderItems[i] = obj.GetComponent<OrderItemUI>();
                    if (_orderItems[i] == null)
                        _orderItems[i] = obj.AddComponent<OrderItemUI>();
                }
            }

            RefreshOrders();
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
            if (_orderItems == null) return;
            var state = GameLoop.Instance.CurrentState;
            int idx = 0;
            foreach (var order in state.Orders)
            {
                if (idx >= _orderItems.Length) break;
                if (_orderItems[idx] != null)
                {
                    _orderItems[idx].gameObject.SetActive(true);
                    _orderItems[idx].UpdateOrder(order.Value);
                }
                idx++;
            }
            for (int i = idx; i < _orderItems.Length; i++)
            {
                if (_orderItems[i] != null)
                    _orderItems[i].gameObject.SetActive(false);
            }
        }
    }
}
