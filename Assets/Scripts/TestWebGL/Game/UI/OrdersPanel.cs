using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 订单面板
    /// </summary>
    public class OrdersPanel : MonoBehaviour
    {
        public RectTransform panelRect;
        public Image backgroundImage;
        public TextMeshProUGUI titleText;
        public Button closeButton;
        public ScrollRect ordersScrollRect;
        public RectTransform ordersContent;
        public TextMeshProUGUI orderTitleText;
        public TextMeshProUGUI orderDescriptionText;
        public TextMeshProUGUI orderRewardText;
        public TextMeshProUGUI orderTimeText;
        public TextMeshProUGUI orderStatusText;
        public Button completeOrderButton;
        public Button activeOrdersTab;
        public Button completedOrdersTab;
        public Image activeTabIndicator;
        public Image completedTabIndicator;
    }
}
