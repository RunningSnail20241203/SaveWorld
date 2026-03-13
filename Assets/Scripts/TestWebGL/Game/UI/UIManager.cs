using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TestWebGL.Game.Grid;
using TestWebGL.Game.Player;
using TestWebGL.Game.Items;
using TestWebGL.Game.Order;

namespace TestWebGL.Game.UI
{
    /// <summary>
    /// UI管理器 - 负责所有UI组件的管理和协调
    /// 单例模式，管理游戏界面的显示和交互
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager s_instance;

        public static UIManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    var go = new GameObject("UIManager");
                    s_instance = go.AddComponent<UIManager>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
        }

        // UI预制件引用
        [Header("UI预制件")]
        public GameObject gridUIPrefab;
        public GameObject playerInfoPanelPrefab;
        public GameObject controlPanelPrefab;
        public GameObject itemDetailPopupPrefab;
        public GameObject settingsPanelPrefab;
        public GameObject ordersPanelPrefab;
        public GameObject achievementPanelPrefab;

        // UI组件引用
        [Header("主要UI面板")]
        public GameObject mainCanvas;
        public GridUI gridUI;
        public PlayerInfoPanel playerInfoPanel;
        public ControlPanel controlPanel;
        public ItemDetailPopup itemDetailPopup;
        public SettingsPanel settingsPanel;
        public OrdersPanel ordersPanel;
        public AchievementPanel achievementPanel;

        // UI状态
        private bool _isInitialized = false;

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 初始化UI系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            Debug.Log("[UIManager] 初始化UI系统...");

            // 初始化物品图标管理器
            ItemIconManager.Instance.PreloadAllIcons();

            // 查找或创建主Canvas
            if (mainCanvas == null)
            {
                mainCanvas = CreateMainCanvas();
            }

            // 初始化各个UI组件
            InitializeGridUI();
            InitializePlayerInfoPanel();
            InitializeControlPanel();
            InitializeItemDetailPopup();
            InitializeSettingsPanel();
            InitializeOrdersPanel();
            InitializeAchievementPanel();

            _isInitialized = true;
            Debug.Log("[UIManager] UI系统初始化完成");
        }

        /// <summary>
        /// 创建主Canvas
        /// </summary>
        private GameObject CreateMainCanvas()
        {
            // 创建Canvas GameObject
            GameObject canvasGO = new GameObject("MainCanvas");
            canvasGO.transform.SetParent(transform);

            // 添加Canvas组件
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            // 添加CanvasScaler
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 添加GraphicRaycaster
            canvasGO.AddComponent<GraphicRaycaster>();

            return canvasGO;
        }

        /// <summary>
        /// 从预制件实例化UI组件
        /// </summary>
        private T InstantiateFromPrefab<T>(GameObject prefab, string name) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[UIManager] {name}预制件未设置，将动态创建");
                GameObject go = new GameObject(name);
                go.transform.SetParent(mainCanvas.transform, false);
                return go.AddComponent<T>();
            }

            GameObject instance = Object.Instantiate(prefab, mainCanvas.transform);
            instance.name = name;
            T component = instance.GetComponent<T>();
            if (component == null)
            {
                component = instance.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// 初始化网格UI
        /// </summary>
        private void InitializeGridUI()
        {
            if (gridUI == null)
            {
                gridUI = InstantiateFromPrefab<GridUI>(gridUIPrefab, "GridUI");
            }
            gridUI.Initialize();
        }

        /// <summary>
        /// 初始化玩家信息面板
        /// </summary>
        private void InitializePlayerInfoPanel()
        {
            if (playerInfoPanel == null)
            {
                playerInfoPanel = InstantiateFromPrefab<PlayerInfoPanel>(playerInfoPanelPrefab, "PlayerInfoPanel");
            }
            playerInfoPanel.Initialize();
        }

        /// <summary>
        /// 初始化控制面板
        /// </summary>
        private void InitializeControlPanel()
        {
            if (controlPanel == null)
            {
                controlPanel = InstantiateFromPrefab<ControlPanel>(controlPanelPrefab, "ControlPanel");
            }
            controlPanel.Initialize();
        }

        /// <summary>
        /// 初始化物品详情弹窗
        /// </summary>
        private void InitializeItemDetailPopup()
        {
            if (itemDetailPopup == null)
            {
                itemDetailPopup = InstantiateFromPrefab<ItemDetailPopup>(itemDetailPopupPrefab, "ItemDetailPopup");
            }
            itemDetailPopup.Initialize();
        }

        /// <summary>
        /// 初始化设置面板
        /// </summary>
        private void InitializeSettingsPanel()
        {
            if (settingsPanel == null)
            {
                settingsPanel = InstantiateFromPrefab<SettingsPanel>(settingsPanelPrefab, "SettingsPanel");
            }
            settingsPanel.Initialize();
        }

        /// <summary>
        /// 初始化订单面板
        /// </summary>
        private void InitializeOrdersPanel()
        {
            if (ordersPanel == null)
            {
                ordersPanel = InstantiateFromPrefab<OrdersPanel>(ordersPanelPrefab, "OrdersPanel");
            }
            ordersPanel.Initialize();
        }

        /// <summary>
        /// 显示物品详情弹窗
        /// </summary>
        public void ShowItemDetail(GridCell cell)
        {
            if (itemDetailPopup != null)
            {
                itemDetailPopup.Show(cell);
            }
        }

        /// <summary>
        /// 隐藏物品详情弹窗
        /// </summary>
        public void HideItemDetail()
        {
            if (itemDetailPopup != null)
            {
                itemDetailPopup.Hide();
            }
        }

        /// <summary>
        /// 显示设置面板
        /// </summary>
        public void ShowSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.Show();
            }
        }

        /// <summary>
        /// 显示订单面板
        /// </summary>
        public void ShowOrders()
        {
            if (ordersPanel != null)
            {
                ordersPanel.Show();
            }
        }

        /// <summary>
        /// 刷新所有UI显示
        /// </summary>
        public void RefreshAllUI()
        {
            if (gridUI != null) gridUI.Refresh();
            if (playerInfoPanel != null) playerInfoPanel.Refresh();
            if (controlPanel != null) controlPanel.Refresh();
            if (ordersPanel != null) ordersPanel.Refresh();
        }

        /// <summary>
        /// 获取UI组件
        /// </summary>
        public GridUI GetGridUI() => gridUI;
        public PlayerInfoPanel GetPlayerInfoPanel() => playerInfoPanel;
        public ControlPanel GetControlPanel() => controlPanel;
        public ItemDetailPopup GetItemDetailPopup() => itemDetailPopup;
        public SettingsPanel GetSettingsPanel() => settingsPanel;
        public OrdersPanel GetOrdersPanel() => ordersPanel;
        public AchievementPanel GetAchievementPanel() => achievementPanel;

        #region 成就面板管理

        /// <summary>
        /// 初始化成就面板
        /// </summary>
        private void InitializeAchievementPanel()
        {
            if (achievementPanel != null)
            {
                achievementPanel.Initialize();
                Debug.Log("[UIManager] 成就面板已初始化");
            }
            else
            {
                Debug.LogWarning("[UIManager] 成就面板引用为空");
            }
        }

        /// <summary>
        /// 显示成就面板
        /// </summary>
        public void ShowAchievements()
        {
            if (achievementPanel != null)
            {
                achievementPanel.Show();
            }
        }

        #endregion
    }
}