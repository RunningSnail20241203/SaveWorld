using UnityEngine;

namespace SaveWorld.Game.Core
{
    /// <summary>
    /// 游戏主循环入口
    /// 所有系统的根
    /// </summary>
    public sealed class GameLoop : MonoBehaviour
    {
        public static GameLoop Instance { get; private set; }

        public EventBus EventBus { get; private set; }
        public GameState CurrentState { get; private set; }

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

            Debug.Log("✅ Game Loop 初始化完成");
        }

        private void Update()
        {
            // 处理所有事件
            EventBus.ProcessEvents();
        }
    }
}