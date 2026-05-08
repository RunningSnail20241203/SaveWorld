using System;
using UnityEngine;
using UnityEngine.UI;
using SaveWorld.Game.Order;

namespace SaveWorld.Game.UI
{
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

        private OrderData? _currentOrder;

        public void UpdateOrder(OrderData order)
        {
            _currentOrder = order;

            RequireItemIcon.sprite = Items.ItemIconManager.Instance.GetItemIcon(order.RequireItem);
            RewardExpText.text = order.RewardExp.ToString();
            RewardGoldText.text = order.RewardGold.ToString();

            DateTime expireDateTime = DateTimeOffset.FromUnixTimeSeconds(order.ExpireTime).LocalDateTime;
            TimeSpan remaining = expireDateTime - DateTime.Now;
            TimeLeftText.text = $"{remaining.Hours:D2}:{remaining.Minutes:D2}";

            // TODO: V2 迁移 - 通过 EventBus 查询背包中是否有该物品
            SubmitButton.interactable = !order.IsCompleted;
            SubmitButton.onClick.RemoveAllListeners();
            SubmitButton.onClick.AddListener(OnSubmitClicked);

            RefreshButton.onClick.RemoveAllListeners();
            RefreshButton.onClick.AddListener(OnRefreshClicked);
        }

        private void OnSubmitClicked()
        {
            if (_currentOrder.HasValue)
            {
                // TODO: V2 迁移 - 通过 EventBus 发布订单提交请求
                Debug.Log($"[OrderUI] 提交订单: {_currentOrder.Value.OrderId}");
            }
        }

        private void OnRefreshClicked()
        {
            if (_currentOrder.HasValue)
            {
                // TODO: V2 迁移 - 通过 EventBus 请求刷新订单
                Debug.Log($"[OrderUI] 刷新订单: {_currentOrder.Value.OrderId}");
            }
        }
    }
}
