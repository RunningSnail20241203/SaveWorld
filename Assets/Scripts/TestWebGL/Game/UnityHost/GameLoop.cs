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

            // 创建 WXSDKManagerHandler 兜底接收器
            // 解决 WebGL Template 中 WXWASMSDK.SendMessage('WXSDKManagerHandler') 目标不存在的问题
            var wxHandler = new GameObject("WXSDKManagerHandler");
            wxHandler.AddComponent<WXSDKHandler>(); // 空脚本，接收 SendMessage
            DontDestroyOnLoad(wxHandler);

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
        /// 确保只调用一次，使用协程异步加载避免首帧卡顿
        /// </summary>
        private void InitOfflineMode()
        {
            if (_systemsInitialized)
            {
                Debug.LogWarning("[GameLoop] 游戏系统已初始化，跳过重复调用");
                return;
            }

            _systemsInitialized = true;
            StartCoroutine(InitializeGameSystemsCoroutine());
        }

        /// <summary>
        /// 异步初始化游戏业务系统 - 分帧加载资源
        /// </summary>
        private IEnumerator InitializeGameSystemsCoroutine()
        {
            Debug.Log("[GameLoop] 开始异步初始化游戏系统...");

            // === Phase 1: 纯逻辑系统初始化（无资源加载）===
            bool useWeChat = WeChatManager != null && WeChatManager.IsInitialized;
            var storageSystem = new SaveWorld.Game.Storage.StorageSystem(useWeChat);
            StateMutator = new StateMutator(EventBus, CurrentState, storageSystem);
            StateMutator.LoadSavedState();

            AchievementSystem = new AchievementSystem(EventBus, StateMutator);
            AchievementSystem.InitializeAchievements();

            GridManager = new GridManager();
            GridManager.Initialize();

            CraftingEngine = new CraftingEngine();
            CraftingEngine.Initialize(GridManager);

            PlayerManager.Instance.Initialize();
            AudioManager = new AudioManager(EventBus);
            FeedbackSystem.Initialize(EventBus);
            AnalyticsSystem = new AnalyticsSystem(EventBus);
            SocialSystem = new SocialSystem(EventBus, StateMutator);

            CloudStorage = new SaveWorld.Game.Storage.CloudStorageSystem(
                EventBus, StateMutator, storageSystem, useWeChat);

            UIManager = new UIManager(EventBus, StateMutator);

            Debug.Log("[GameLoop] Phase 1 完成：纯逻辑系统初始化");

            // === Phase 2: 异步预加载物品图标 ===
            yield return ItemIconManager.Instance.PreloadAllIconsCoroutine(
                (loaded, total, progress) => {
                    // 可在此处更新加载进度UI
                    if ((loaded % 20) == 0 || loaded == total)
                        Debug.Log($"[GameLoad] 图标加载进度: {loaded}/{total} ({progress:P0})");
                },
                () => Debug.Log("[GameLoop] Phase 2 完成：图标预加载完成"));

            // === Phase 3: 分帧加载UI面板 ===
            yield return LoadUIPanelsCoroutine();

            // === Phase 4: 发布启动事件 ===
            EventBus.Publish(new GameStartedEvent());

            Debug.Log("[GameLoop] 游戏系统异步初始化完成");
        }

        /// <summary>
        /// 分帧异步加载UI面板
        /// 每帧加载1个预制体，避免主线程阻塞
        /// </summary>
        private IEnumerator LoadUIPanelsCoroutine()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[GameLoop] 未找到Canvas，跳过UI加载");
                yield break;
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
                    backpackUI.CellPrefab = oldGridUI.cellPrefab;
                backpackUI.ParentCanvas = canvas;
                
                // 分帧实例化63个格子（每帧7个，共9帧）
                yield return StartCoroutine(backpackUI.InitializeAsync());
                
                Debug.Log("[GameLoop] GridUI 加载完成");
            }
            else
            {
                Debug.LogError("[GameLoop] GridUI 预制体未找到");
            }

            yield return null; // 每个面板间让出一帧

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

            yield return null; // 每个面板间让出一帧

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
