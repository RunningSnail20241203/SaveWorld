using UnityEngine;
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

            Debug.Log("[GameLoop] Awake 完成，等待微信SDK初始化...");
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

            // 初始化云存储系统（监听 GameStartedEvent 自动同步）
            CloudStorage = new SaveWorld.Game.Storage.CloudStorageSystem(
                EventBus, StateMutator, storageSystem, useWeChat);

            // 发布游戏启动事件（触发云同步、成就等子系统）
            EventBus.Publish(new GameStartedEvent());

            Debug.Log("[GameLoop] 游戏系统初始化完成");
        }

        private void Update()
        {
            // 处理所有事件
            EventBus.ProcessEvents();
        }
    }
}
