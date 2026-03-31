using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestWebGL.Game.Core;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// 控制面板 - 包含游戏的主要操作按钮
    /// 探索、设置、订单等功能按钮
    /// </summary>
    public class ControlPanel : MonoBehaviour
    {
        [Header("按钮组件")]
        public Button exploreButton;
        public Button settingsButton;
        public Button ordersButton;
        public Button achievementsButton;
        public Button saveButton;

        [Header("按钮文本")]
        public TextMeshProUGUI exploreButtonText;
        public TextMeshProUGUI settingsButtonText;
        public TextMeshProUGUI ordersButtonText;
        public TextMeshProUGUI achievementsButtonText;
        public TextMeshProUGUI saveButtonText;

        [Header("面板设置")]
        public Vector2 panelSize = new Vector2(400, 100);
        public Vector2 panelPosition = new Vector2(0, -400);

        private GameManager _gameManager;

        /// <summary>
        /// 初始化控制面板
        /// </summary>
        public void Initialize()
        {
            _gameManager = GameManager.Instance;

            if (GetComponent<RectTransform>() == null)
            {
                CreatePanel();
            }

            CreateButtons();
            SetupButtonEvents();
            Refresh();

            Debug.Log("[ControlPanel] 控制面板初始化完成");
        }

        /// <summary>
        /// 创建面板
        /// </summary>
        private void CreatePanel()
        {
            RectTransform panelRect = gameObject.AddComponent<RectTransform>();

            // 设置面板位置和大小
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = panelPosition;
            panelRect.sizeDelta = panelSize;

            // 添加背景
            Image background = gameObject.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // 添加水平布局
            HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
        }

        /// <summary>
        /// 创建按钮
        /// </summary>
        private void CreateButtons()
        {
            // 探索按钮
            if (exploreButton == null)
            {
                exploreButton = CreateButton("ExploreButton", "探索");
            }

            // 设置按钮
            if (settingsButton == null)
            {
                settingsButton = CreateButton("SettingsButton", "设置");
            }

            // 订单按钮
            if (ordersButton == null)
            {
                ordersButton = CreateButton("OrdersButton", "订单");
            }

            // 成就按钮
            if (achievementsButton == null)
            {
                achievementsButton = CreateButton("AchievementsButton", "成就");
            }

            // 保存按钮
            if (saveButton == null)
            {
                saveButton = CreateButton("SaveButton", "保存");
            }
        }

        /// <summary>
        /// 创建单个按钮
        /// </summary>
        private Button CreateButton(string name, string text)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(transform, false);

            // 添加RectTransform组件（UI元素必须要有）
            RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
            
            // 添加按钮组件
            Button button = buttonGO.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
            colors.selectedColor = colors.normalColor;
            button.colors = colors;

            // 设置按钮大小
            buttonRect.sizeDelta = new Vector2(80, 60);

            // 创建文本
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 16;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.color = Color.white;

            // 设置文本RectTransform
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // 根据按钮类型设置对应的文本引用
            switch (name)
            {
                case "ExploreButton":
                    exploreButtonText = textComponent;
                    break;
                case "SettingsButton":
                    settingsButtonText = textComponent;
                    break;
                case "OrdersButton":
                    ordersButtonText = textComponent;
                    break;
                case "AchievementsButton":
                    achievementsButtonText = textComponent;
                    break;
                case "SaveButton":
                    saveButtonText = textComponent;
                    break;
            }

            return button;
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtonEvents()
        {
            if (exploreButton != null)
            {
                exploreButton.onClick.AddListener(OnExploreButtonClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }

            if (ordersButton != null)
            {
                ordersButton.onClick.AddListener(OnOrdersButtonClicked);
            }

            if (achievementsButton != null)
            {
                achievementsButton.onClick.AddListener(OnAchievementsButtonClicked);
            }

            if (saveButton != null)
            {
                saveButton.onClick.AddListener(OnSaveButtonClicked);
            }
        }

        /// <summary>
        /// 刷新按钮状态
        /// </summary>
        public void Refresh()
        {
            // 更新探索按钮状态
            UpdateExploreButton();
        }

        /// <summary>
        /// 更新探索按钮状态
        /// </summary>
        private void UpdateExploreButton()
        {
            if (exploreButton == null || exploreButtonText == null) return;

            var playerStats = _gameManager.GetPlayerManager().GetStatistics();
            bool canExplore = playerStats.currentStamina >= 1;

            exploreButton.interactable = canExplore;

            if (canExplore)
            {
                exploreButtonText.text = "探索";
                exploreButtonText.color = Color.white;
            }
            else
            {
                exploreButtonText.text = "无体力";
                exploreButtonText.color = Color.gray;
            }
        }

        /// <summary>
        /// 探索按钮点击事件
        /// </summary>
        private void OnExploreButtonClicked()
        {
            var explorationEngine = _gameManager.GetExplorationEngine();
            if (explorationEngine.TryExplore())
            {
                Refresh(); // 刷新按钮状态
            }
        }

        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            UIManager.Instance.ShowSettings();
        }

        /// <summary>
        /// 订单按钮点击事件
        /// </summary>
        private void OnOrdersButtonClicked()
        {
            UIManager.Instance.ShowOrders();
        }

        /// <summary>
        /// 成就按钮点击事件
        /// </summary>
        private void OnAchievementsButtonClicked()
        {
            UIManager.Instance.ShowAchievements();
        }

        /// <summary>
        /// 保存按钮点击事件
        /// </summary>
        private void OnSaveButtonClicked()
        {
            _gameManager.SavePlayerData();
            _gameManager.SaveGridData();

            // 显示保存成功反馈
            if (saveButtonText != null)
            {
                string originalText = saveButtonText.text;
                saveButtonText.text = "已保存";
                saveButtonText.color = Color.green;

                // 1秒后恢复原文本
                StartCoroutine(ResetSaveButtonText(originalText));
            }
        }

        /// <summary>
        /// 重置保存按钮文本的协程
        /// </summary>
        private System.Collections.IEnumerator ResetSaveButtonText(string originalText)
        {
            yield return new WaitForSeconds(1f);

            if (saveButtonText != null)
            {
                saveButtonText.text = originalText;
                saveButtonText.color = Color.white;
            }
        }
    }
}