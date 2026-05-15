using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 设置面板
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        public RectTransform panelRect;
        public Image backgroundImage;
        public TextMeshProUGUI titleText;
        public Button applyButton;
        public Button resetButton;
        public Button closeButton;
    }
}
