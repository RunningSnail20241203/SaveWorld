using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestWebGL.Game.Player;
using TestWebGL.Game.Core;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// 玩家信息面板 - 显示玩家等级、经验、体力等信息
    /// </summary>
    public class PlayerInfoPanel : MonoBehaviour
    {
        [Header("UI组件")]
        public RectTransform panelRect;
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI experienceText;
        public Slider experienceSlider;
        public TextMeshProUGUI staminaText;
        public Slider staminaSlider;
        public TextMeshProUGUI playTimeText;

        [Header("面板设置")]
        public Vector2 panelSize = new Vector2(400, 200);
        public Vector2 panelPosition = new Vector2(0, 400);

        private PlayerManager _playerManager;

        /// <summary>
        /// 初始化玩家信息面板
        /// </summary>
        public void Initialize()
        {
            _playerManager = GameManager.Instance.GetPlayerManager();

            // 从预制件获取RectTransform
            if (panelRect == null)
            {
                panelRect = GetComponent<RectTransform>();
            }

            // 从预制件获取UI组件引用
            if (playerNameText == null)
                playerNameText = transform.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
            if (levelText == null)
                levelText = transform.Find("LevelInfo")?.GetComponent<TextMeshProUGUI>();
            if (experienceText == null)
                experienceText = transform.Find("ExperienceBar/ExperienceText")?.GetComponent<TextMeshProUGUI>();
            if (experienceSlider == null)
                experienceSlider = transform.Find("ExperienceBar/Background")?.GetComponent<Slider>();
            if (staminaText == null)
                staminaText = transform.Find("StaminaBar/StaminaText")?.GetComponent<TextMeshProUGUI>();
            if (staminaSlider == null)
                staminaSlider = transform.Find("StaminaBar/Background")?.GetComponent<Slider>();
            if (playTimeText == null)
                playTimeText = transform.Find("PlayTime")?.GetComponent<TextMeshProUGUI>();

            Refresh();

            // 订阅玩家数据变化事件
            _playerManager.OnLevelChanged += OnPlayerLevelChanged;
            _playerManager.OnStaminaChanged += OnPlayerStaminaChanged;
            _playerManager.OnExperienceGained += OnExperienceGained;

            Debug.Log("[PlayerInfoPanel] 玩家信息面板初始化完成");
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void Refresh()
        {
            if (_playerManager == null) return;

            var stats = _playerManager.GetStatistics();
            var playerData = _playerManager.GetPlayerData();

            // 更新玩家名称
            if (playerNameText != null)
            {
                playerNameText.text = playerData.playerName;
            }

            // 更新等级
            if (levelText != null)
            {
                levelText.text = $"等级 {stats.level}";
            }

            // 更新经验
            if (experienceText != null && experienceSlider != null)
            {
                int currentLevelExp = GetExperienceForLevel(stats.level);
                int nextLevelExp = GetExperienceForLevel(stats.level + 1);
                int currentLevelProgress = stats.experience - currentLevelExp;

                experienceText.text = $"经验: {currentLevelProgress}/{nextLevelExp - currentLevelExp}";
                experienceSlider.value = (float)currentLevelProgress / (nextLevelExp - currentLevelExp);
            }

            // 更新体力
            if (staminaText != null && staminaSlider != null)
            {
                staminaText.text = $"体力: {stats.currentStamina}/{stats.maxStamina}";
                staminaSlider.value = (float)stats.currentStamina / stats.maxStamina;
            }

            // 更新游戏时长
            if (playTimeText != null)
            {
                playTimeText.text = $"游戏时长: {stats.playTime.Hours:D2}:{stats.playTime.Minutes:D2}:{stats.playTime.Seconds:D2}";
            }
        }

        /// <summary>
        /// 获取指定等级需要的经验值
        /// </summary>
        private int GetExperienceForLevel(int level)
        {
            // 简单的经验计算公式：每级需要 level * 100 经验
            return level * 100;
        }

        /// <summary>
        /// 玩家等级变化事件
        /// </summary>
        private void OnPlayerLevelChanged(int newLevel, int oldLevel)
        {
            Refresh();
        }

        /// <summary>
        /// 玩家体力变化事件
        /// </summary>
        private void OnPlayerStaminaChanged(int newStamina, int maxStamina)
        {
            Refresh();
        }

        /// <summary>
        /// 玩家获得经验事件
        /// </summary>
        private void OnExperienceGained(int amount, string reason)
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (_playerManager != null)
            {
                _playerManager.OnLevelChanged -= OnPlayerLevelChanged;
                _playerManager.OnStaminaChanged -= OnPlayerStaminaChanged;
                _playerManager.OnExperienceGained -= OnExperienceGained;
            }
        }
    }
}