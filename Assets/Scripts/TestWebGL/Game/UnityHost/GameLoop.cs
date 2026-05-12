using UnityEngine;
using System.Collections;
using SaveWorld.Game.Achievement;
using SaveWorld.Game.Analytics;
using SaveWorld.Game.Audio;
using SaveWorld.Game.Crafting;
using SaveWorld.Game.Feedback;
using SaveWorld.Game.Grid;
using SaveWorld.Game.Player;
using SaveWorld.Game.Social;
using SaveWorld.Game.UI;
using SaveWorld.Game.WeChat;

namespace SaveWorld.Game.Core
{
    /// <summary>
    /// 游戏主循环入口 - 所有系统的根
    /// 微信小游戏启动流程: WX.InitSDK → 微信登录 → 游戏系统初始化
    /// 非微信环境直接进入离线模式
    /// </summary>
    public sealed class GameLoop : MonoBehaviour
    {
        public static GameLoop Instance { get; private set; }

        public EventBus EventBus { get; private set; }
        public GameState CurrentState { get; private set; }
        public StateMutator StateMutator { get; private set; }
        public WeChatManager WeChatManager { get; private set; }
        public SaveWorld.Game.Storage.CloudStorageSystem CloudStorage { get; private set; }
        public UIManager UIManager { get; private set; }
        public AchievementSystem AchievementSystem { get; private set; }
        public GridManager GridManager { get; private set; }
        public CraftingEngine CraftingEngine { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public AnalyticsSystem AnalyticsSystem { get; private set; }
        public SocialSystem SocialSystem { get; private set; }

        private bool _systemsInitialized = false;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 初始化核心系统
            EventBus = new EventBus();
            CurrentState = GameState.CreateInitial();

            // 初始化微信管理器（内部调用 WX.InitSDK，异步完成后再启动游戏）
            WeChatManager = new WeChatManager(EventBus);

            // 监听微信SDK初始化与登录事件
            EventBus.Listen<WeChatInitializedEvent>(OnWeChatInitialized);
            EventBus.Listen<WeChatLoginCompletedEvent>(OnWeChatLoginCompleted);

            // 微信SDK超时兜底：2秒后如果还没初始化成功，自动进入离线模式
            StartCoroutine(WeChatTimeoutFallback());

            Debug.Log("[GameLoop] Awake 完成，等待微信SDK初始化...");
        }

        private IEnumerator WeChatTimeoutFallback()
        {
            yield return new WaitForSeconds(2f);
            if (!_systemsInitialized)
            {
                Debug.LogWarning("[GameLoop] 微信SDK初始化超时（2秒），自动进入离线模式");
                InitOfflineMode();
            }
        }

        /// <summary>
        /// 微信SDK初始化回调（成功/失败两种路径）
        /// 成功 → 发起微信登录
        /// 失败 → 直接进入离线模式
        /// </summary>
        private void OnWeChatInitialized(WeChatInitializedEvent evt)
        {
            if (evt.Success)
            {
                Debug.Log("[GameLoop] 微信SDK初始化成功，开始登录...");
                WeChatManager.Login();
            }
            else
            {
                Debug.LogWarning("[GameLoop] 微信SDK初始化失败，以离线模式运行");
                InitOfflineMode();
            }
        }

        /// <summary>
        /// 微信登录完成回调
        /// 无论登录成功或失败，都继续启动游戏系统
        /// </summary>
        private void OnWeChatLoginCompleted(WeChatLoginCompletedEvent evt)
        {
            if (evt.Success)
            {
                Debug.Log($"[GameLoop] 微信登录成功, OpenID: {WeChatManager.OpenId}");
            }
            else
            {
                Debug.LogWarning("[GameLoop] 微信登录失败，以离线模式运行");
            }

            InitOfflineMode();
        }

        /// <summary>
        /// 离线模式/兜底启动：初始化游戏业务系统
        /// 确保只调用一次
        /// </summary>
        private void InitOfflineMode()
        {
            if (_systemsInitialized)
            {
                Debug.LogWarning("[GameLoop] 游戏系统已初始化，跳过重复调用");
                return;
            }

            _systemsInitialized = true;
            InitializeGameSystems();
        }

        private void InitializeGameSystems()
        {
            // 初始化 StorageSystem（微信环境使用WX.Storage，其他使用PlayerPrefs）
            bool useWeChat = WeChatManager != null && WeChatManager.IsInitialized;
            var storageSystem = new SaveWorld.Game.Storage.StorageSystem(useWeChat);
            StateMutator = new StateMutator(EventBus, CurrentState, storageSystem);

            // 加载存档状态（恢复玩家数据、订单、成就等）
            StateMutator.LoadSavedState();

            // 初始化成就系统
            AchievementSystem = new AchievementSystem(EventBus, StateMutator);
            AchievementSystem.InitializeAchievements();

            // 初始化格子系统
            GridManager = new GridManager();
            GridManager.Initialize();

            // 初始化合成引擎
            CraftingEngine = new CraftingEngine();
            CraftingEngine.Initialize(GridManager);

            // 初始化玩家管理器
            PlayerManager.Instance.Initialize();

            // 初始化音频系统
            AudioManager = new AudioManager(EventBus);

            // 初始化反馈系统
            FeedbackSystem.Initialize(EventBus);

            // 初始化数据分析系统
            AnalyticsSystem = new AnalyticsSystem(EventBus);

            // 初始化社交系统
            SocialSystem = new SocialSystem(EventBus, StateMutator);

            // 初始化云存储系统（监听 GameStartedEvent 自动同步）
            CloudStorage = new SaveWorld.Game.Storage.CloudStorageSystem(
                EventBus, StateMutator, storageSystem, useWeChat);

            // 初始化UI管理器
            UIManager = new UIManager(EventBus, StateMutator);

            // 加载并初始化所有UI面板
            LoadUIPanels();

            // 发布游戏启动事件（触发云同步）
            EventBus.Publish(new GameStartedEvent());

            Debug.Log("[GameLoop] 游戏系统初始化完成");
        }

        private void LoadUIPanels()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[GameLoop] 未找到Canvas，跳过UI加载");
                return;
            }

            // 加载 GridUI (背包网格)
            var gridUIPrefab = Resources.Load<GameObject>("Prefabs/UI/GridUI");
            if (gridUIPrefab != null)
            {
                var gridUIObj = Instantiate(gridUIPrefab, canvas.transform);
                gridUIObj.name = "GridUI";

                var oldGridUI = gridUIObj.GetComponent<GridUI>();
                var backpackUI = gridUIObj.AddComponent<BackpackUI>();
                if (oldGridUI != null)
                {
                    backpackUI.CellPrefab = oldGridUI.cellPrefab;
                }
                backpackUI.ParentCanvas = canvas;
                backpackUI.Initialize();
                Debug.Log("[GameLoop] GridUI 加载完成");
            }
            else
            {
                Debug.LogError("[GameLoop] GridUI 预制体未找到");
            }

            // 加载 PlayerInfoPanel (玩家信息面板)
            var playerInfoPrefab = Resources.Load<GameObject>("Prefabs/UI/PlayerInfoPanel");
            if (playerInfoPrefab != null)
            {
                var playerInfoObj = Instantiate(playerInfoPrefab, canvas.transform);
                playerInfoObj.name = "PlayerInfoPanel";

                var rect = playerInfoObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(0.5f, 1);
                    rect.anchoredPosition = new Vector2(0, 0);
                    rect.sizeDelta = new Vector2(0, 80);
                }

                var playerStatusUI = playerInfoObj.AddComponent<PlayerStatusUI>();
                playerStatusUI.Initialize();
                Debug.Log("[GameLoop] PlayerInfoPanel 加载完成");
            }
            else
            {
                Debug.LogError("[GameLoop] PlayerInfoPanel 预制体未找到");
            }

            // 加载 ControlPanel (底部按钮栏)
            var controlPanelPrefab = Resources.Load<GameObject>("Prefabs/UI/ControlPanel");
            if (controlPanelPrefab != null)
            {
                var controlPanelObj = Instantiate(controlPanelPrefab, canvas.transform);
                controlPanelObj.name = "ControlPanel";

                var rect = controlPanelObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(1, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                    rect.anchoredPosition = new Vector2(0, 10);
                    rect.sizeDelta = new Vector2(-20, 60);
                }

                Debug.Log("[GameLoop] ControlPanel 加载完成");
            }
            else
            {
                Debug.LogError("[GameLoop] ControlPanel 预制体未找到");
            }

            Debug.Log("[GameLoop] UI面板加载完成");
        }

        private void Update()
        {
            // 处理所有事件
            EventBus.ProcessEvents();
        }
    }
}
