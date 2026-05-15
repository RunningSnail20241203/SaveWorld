using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 物品详情弹窗
    /// </summary>
    public class ItemDetailPopup : MonoBehaviour
    {
        public Image backgroundImage;
        public Image itemIcon;
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemDescriptionText;
        public Button actionButton;
        public Button closeButton;
    }
}
