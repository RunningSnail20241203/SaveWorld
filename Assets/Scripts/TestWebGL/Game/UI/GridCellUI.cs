using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 网格格子UI - 单个背包格
    /// </summary>
    public class GridCellUI : MonoBehaviour
    {
        public Image backgroundImage;
        public Image itemIcon;
        public TextMeshProUGUI itemCountText;
        public TextMeshProUGUI lockLevelText;
        public Button cellButton;
    }
}
