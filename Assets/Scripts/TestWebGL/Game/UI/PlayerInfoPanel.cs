using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 玩家信息面板
    /// </summary>
    public class PlayerInfoPanel : MonoBehaviour
    {
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI experienceText;
        public Slider experienceSlider;
        public TextMeshProUGUI staminaText;
        public Slider staminaSlider;
        public TextMeshProUGUI playTimeText;
    }
}
