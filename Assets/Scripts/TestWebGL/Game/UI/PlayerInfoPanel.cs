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

            if (panelRect == null)
            {
                CreatePanel();
            }

            CreateUIComponents();
            Refresh();

            // 订阅玩家数据变化事件
            _playerManager.OnLevelChanged += OnPlayerLevelChanged;
            _playerManager.OnStaminaChanged += OnPlayerStaminaChanged;
            _playerManager.OnExperienceGained += OnExperienceGained;

            Debug.Log("[PlayerInfoPanel] 玩家信息面板初始化完成");
        }

        /// <summary>
        /// 创建面板
        /// </summary>
        private void CreatePanel()
        {
            panelRect = GetComponent<RectTransform>();
            if (panelRect == null)
            {
                panelRect = gameObject.AddComponent<RectTransform>();
            }

            // 设置面板位置和大小
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = panelPosition;
            panelRect.sizeDelta = panelSize;

            // 添加背景
            Image background = gameObject.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }

        /// <summary>
        /// 创建UI组件
        /// </summary>
        private void CreateUIComponents()
        {
            // 创建垂直布局
            VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 5;
            layout.childAlignment = TextAnchor.UpperCenter;

            // 玩家名称
            CreatePlayerNameText();

            // 等级信息
            CreateLevelInfo();

            // 经验条
            CreateExperienceBar();

            // 体力条
            CreateStaminaBar();

            // 游戏时长
            CreatePlayTimeText();
        }

        /// <summary>
        /// 创建玩家名称文本
        /// </summary>
        private void CreatePlayerNameText()
        {
            GameObject nameGO = new GameObject("PlayerName");
            nameGO.transform.SetParent(transform, false);

            playerNameText = nameGO.AddComponent<TextMeshProUGUI>();
            playerNameText.fontSize = 24;
            playerNameText.alignment = TextAlignmentOptions.Center;
            playerNameText.color = Color.white;

            // 设置RectTransform
            RectTransform rect = nameGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(panelSize.x - 20, 30);
        }

        /// <summary>
        /// 创建等级信息
        /// </summary>
        private void CreateLevelInfo()
        {
            GameObject levelGO = new GameObject("LevelInfo");
            levelGO.transform.SetParent(transform, false);

            levelText = levelGO.AddComponent<TextMeshProUGUI>();
            levelText.fontSize = 18;
            levelText.alignment = TextAlignmentOptions.Center;
            levelText.color = Color.yellow;

            // 设置RectTransform
            RectTransform rect = levelGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(panelSize.x - 20, 25);
        }

        /// <summary>
        /// 创建经验条
        /// </summary>
        private void CreateExperienceBar()
        {
            GameObject expGO = new GameObject("ExperienceBar");
            expGO.transform.SetParent(transform, false);

            // 经验条背景
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(expGO.transform, false);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.gray;

            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(panelSize.x - 20, 20);

            // 经验条填充
            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(bgGO.transform, false);
            Image fillImage = fillGO.AddComponent<Image>();
            fillImage.color = Color.blue;

            RectTransform fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // 经验条组件
            experienceSlider = bgGO.AddComponent<Slider>();
            experienceSlider.fillRect = fillRect;
            experienceSlider.minValue = 0;
            experienceSlider.maxValue = 1;
            experienceSlider.interactable = false;

            // 经验文本
            GameObject textGO = new GameObject("ExperienceText");
            textGO.transform.SetParent(expGO.transform, false);

            experienceText = textGO.AddComponent<TextMeshProUGUI>();
            experienceText.fontSize = 14;
            experienceText.alignment = TextAlignmentOptions.Center;
            experienceText.color = Color.white;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(panelSize.x - 20, 20);
        }

        /// <summary>
        /// 创建体力条
        /// </summary>
        private void CreateStaminaBar()
        {
            GameObject staminaGO = new GameObject("StaminaBar");
            staminaGO.transform.SetParent(transform, false);

            // 体力条背景
            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(staminaGO.transform, false);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = Color.gray;

            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(panelSize.x - 20, 20);

            // 体力条填充
            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(bgGO.transform, false);
            Image fillImage = fillGO.AddComponent<Image>();
            fillImage.color = Color.green;

            RectTransform fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // 体力条组件
            staminaSlider = bgGO.AddComponent<Slider>();
            staminaSlider.fillRect = fillRect;
            staminaSlider.minValue = 0;
            staminaSlider.maxValue = 1;
            staminaSlider.interactable = false;

            // 体力文本
            GameObject textGO = new GameObject("StaminaText");
            textGO.transform.SetParent(staminaGO.transform, false);

            staminaText = textGO.AddComponent<TextMeshProUGUI>();
            staminaText.fontSize = 14;
            staminaText.alignment = TextAlignmentOptions.Center;
            staminaText.color = Color.white;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(panelSize.x - 20, 20);
        }

        /// <summary>
        /// 创建游戏时长文本
        /// </summary>
        private void CreatePlayTimeText()
        {
            GameObject timeGO = new GameObject("PlayTime");
            timeGO.transform.SetParent(transform, false);

            playTimeText = timeGO.AddComponent<TextMeshProUGUI>();
            playTimeText.fontSize = 16;
            playTimeText.alignment = TextAlignmentOptions.Center;
            playTimeText.color = Color.cyan;

            // 设置RectTransform
            RectTransform rect = timeGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(panelSize.x - 20, 25);
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