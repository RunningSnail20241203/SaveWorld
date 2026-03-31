using UnityEngine;
using TestWebGL.Game.UI;

namespace TestWebGL.Game.Core
{
    /// <summary>
    /// 场景设置脚本
    /// 确保游戏场景正确配置，可以直接运行
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("自动配置")]
        [Tooltip("是否在启动时自动配置场景")]
        public bool autoSetup = true;

        [Header("UI预制件")]
        [Tooltip("网格UI预制件")]
        public GameObject gridUIPrefab;
        
        [Tooltip("玩家信息面板预制件")]
        public GameObject playerInfoPanelPrefab;
        
        [Tooltip("控制面板预制件")]
        public GameObject controlPanelPrefab;
        
        [Tooltip("物品详情弹窗预制件")]
        public GameObject itemDetailPopupPrefab;
        
        [Tooltip("设置面板预制件")]
        public GameObject settingsPanelPrefab;
        
        [Tooltip("订单面板预制件")]
        public GameObject ordersPanelPrefab;
        
        [Tooltip("成就面板预制件")]
        public GameObject achievementPanelPrefab;

        private void Awake()
        {
            if (autoSetup)
            {
                SetupScene();
            }
        }

        /// <summary>
        /// 设置场景
        /// </summary>
        public void SetupScene()
        {
            Debug.Log("[SceneSetup] 开始配置游戏场景...");

            // 1. 确保GameManager存在
            EnsureGameManager();

            // 2. 确保UIManager存在并配置预制件
            EnsureUIManager();

            // 3. 确保GameEntry存在
            EnsureGameEntry();

            // 4. 配置相机
            SetupCamera();

            // 5. 配置事件系统
            SetupEventSystem();

            Debug.Log("[SceneSetup] 场景配置完成！");
        }

        /// <summary>
        /// 确保GameManager存在
        /// </summary>
        private void EnsureGameManager()
        {
            if (GameManager.Instance == null)
            {
                Debug.Log("[SceneSetup] GameManager不存在，将自动创建");
            }
            else
            {
                Debug.Log("[SceneSetup] GameManager已存在");
            }
        }

        /// <summary>
        /// 确保UIManager存在并配置预制件
        /// </summary>
        private void EnsureUIManager()
        {
            var uiManager = UIManager.Instance;
            
            // 配置预制件引用
            if (gridUIPrefab != null) uiManager.gridUIPrefab = gridUIPrefab;
            if (playerInfoPanelPrefab != null) uiManager.playerInfoPanelPrefab = playerInfoPanelPrefab;
            if (controlPanelPrefab != null) uiManager.controlPanelPrefab = controlPanelPrefab;
            if (itemDetailPopupPrefab != null) uiManager.itemDetailPopupPrefab = itemDetailPopupPrefab;
            if (settingsPanelPrefab != null) uiManager.settingsPanelPrefab = settingsPanelPrefab;
            if (ordersPanelPrefab != null) uiManager.ordersPanelPrefab = ordersPanelPrefab;
            if (achievementPanelPrefab != null) uiManager.achievementPanelPrefab = achievementPanelPrefab;

            Debug.Log("[SceneSetup] UIManager配置完成");
        }

        /// <summary>
        /// 确保GameEntry存在
        /// </summary>
        private void EnsureGameEntry()
        {
            var gameEntry = FindObjectOfType<GameEntry>();
            if (gameEntry == null)
            {
                Debug.Log("[SceneSetup] GameEntry不存在，创建新的GameEntry");
                var go = new GameObject("GameEntry");
                go.AddComponent<GameEntry>();
            }
            else
            {
                Debug.Log("[SceneSetup] GameEntry已存在");
            }
        }

        /// <summary>
        /// 配置相机
        /// </summary>
        private void SetupCamera()
        {
            Camera.main.backgroundColor = new Color(0.2f, 0.3f, 0.4f); // 深蓝灰色
            Camera.main.orthographic = false;
            Camera.main.fieldOfView = 60f;
            Debug.Log("[SceneSetup] 相机配置完成");
        }

        /// <summary>
        /// 配置事件系统
        /// </summary>
        private void SetupEventSystem()
        {
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                Debug.Log("[SceneSetup] 创建EventSystem");
                var go = new GameObject("EventSystem");
                go.AddComponent<UnityEngine.EventSystems.EventSystem>();
                go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            else
            {
                Debug.Log("[SceneSetup] EventSystem已存在");
            }
        }

        /// <summary>
        /// 重新配置场景（编辑器用）
        /// </summary>
        [ContextMenu("重新配置场景")]
        public void ReconfigureScene()
        {
            SetupScene();
        }

        /// <summary>
        /// 测试游戏功能
        /// </summary>
        [ContextMenu("测试游戏功能")]
        public void TestGameFunctionality()
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                Debug.Log("[SceneSetup] 测试游戏功能...");
                gameManager.Debug_PrintGridInfo();
                gameManager.Debug_PrintPlayerInfo();
                Debug.Log("[SceneSetup] 游戏功能测试完成");
            }
            else
            {
                Debug.LogError("[SceneSetup] GameManager不存在，无法测试");
            }
        }
    }
}