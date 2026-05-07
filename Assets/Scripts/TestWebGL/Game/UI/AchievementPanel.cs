using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SaveWorld.Game.UI
{
    /// <summary>
    /// 成就面板
    /// </summary>
    public class AchievementPanel : MonoBehaviour
    {
        public GameObject achievementItemPrefab;
        public TextMeshProUGUI progressText;
        public Transform achievementListContainer;
        public Button closeButton;
    }
}
