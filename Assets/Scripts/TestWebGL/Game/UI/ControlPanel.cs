using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 控制面板 - 底部按钮栏
    /// </summary>
    public class ControlPanel : MonoBehaviour
    {
        public Button exploreButton;
        public Button settingsButton;
        public Button ordersButton;
        public Button achievementsButton;
        public Button saveButton;

        public TextMeshProUGUI exploreButtonText;
        public TextMeshProUGUI settingsButtonText;
        public TextMeshProUGUI ordersButtonText;
        public TextMeshProUGUI achievementsButtonText;
        public TextMeshProUGUI saveButtonText;
    }
}
