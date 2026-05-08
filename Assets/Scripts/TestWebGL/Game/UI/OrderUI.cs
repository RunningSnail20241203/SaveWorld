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

        public override void Initialize()
        {

            // 创建3个订单槽位
            _orderItems = new OrderItemUI[3];
            for (int i = 0; i < 3; i++)
            {
                var obj = UnityEngine.Object.Instantiate(OrderItemPrefab, OrderContainer);
                _orderItems[i] = obj.GetComponent<OrderItemUI>();
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
            // TODO: V2 迁移 - 从 GameState 获取活跃订单并通过 EventBus 刷新
            for (int i = 0; i < 3; i++)
            {
                _orderItems[i].gameObject.SetActive(false);
            }
        }
    }

}
