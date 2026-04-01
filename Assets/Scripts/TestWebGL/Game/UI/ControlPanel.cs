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
        public Vector2 panelSize = new Vector2(500, 120);
        public Vector2 panelPosition = new Vector2(0, -500);

        private GameManager _gameManager;

        /// <summary>
        /// 初始化控制面板
        /// </summary>
        public void Initialize()
        {
            _gameManager = GameManager.Instance;

            // 从预制件加载
            if (GetComponent<RectTransform>() == null)
            {
                LoadFromPrefab();
            }

            SetupButtonEvents();
            Refresh();

            Debug.Log("[ControlPanel] 控制面板初始化完成");
        }

        /// <summary>
        /// 从预制件加载
        /// </summary>
        private void LoadFromPrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/UI/ControlPanel");
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.name = "ControlPanel";
                
                // 获取组件引用
                exploreButton = instance.transform.Find("ButtonContainer/ExploreButton")?.GetComponent<Button>();
                settingsButton = instance.transform.Find("ButtonContainer/SettingsButton")?.GetComponent<Button>();
                ordersButton = instance.transform.Find("ButtonContainer/OrdersButton")?.GetComponent<Button>();
                achievementsButton = instance.transform.Find("ButtonContainer/AchievementsButton")?.GetComponent<Button>();
                saveButton = instance.transform.Find("ButtonContainer/SaveButton")?.GetComponent<Button>();
                
                // 获取按钮文本引用
                if (exploreButton != null)
                    exploreButtonText = exploreButton.GetComponentInChildren<TextMeshProUGUI>();
                if (settingsButton != null)
                    settingsButtonText = settingsButton.GetComponentInChildren<TextMeshProUGUI>();
                if (ordersButton != null)
                    ordersButtonText = ordersButton.GetComponentInChildren<TextMeshProUGUI>();
                if (achievementsButton != null)
                    achievementsButtonText = achievementsButton.GetComponentInChildren<TextMeshProUGUI>();
                if (saveButton != null)
                    saveButtonText = saveButton.GetComponentInChildren<TextMeshProUGUI>();
            }
            else
            {
                Debug.LogError("[ControlPanel] 无法加载ControlPanel预制件");
            }
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