using UnityEngine;
using TestWebGL.Game.Items;
using TestWebGL.Game.Grid;
using TestWebGL.Game.Player;

namespace TestWebGL.Game.Core
{
    /// <summary>
    /// 游戏入口脚本
    /// 挂载在场景中，负责初始化游戏和处理调试输入
    /// 应该在SampleScene.unity中的一个空GameObject上
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        private GameManager _gameManager;

        private void Awake()
        {
            // 获取GameManager实例（自动创建if不存在）
            _gameManager = GameManager.Instance;
        }

        private void Update()
        {
            HandleDebugInput();
        }

        /// <summary>
        /// 处理调试键盘输入
        /// </summary>
        private void HandleDebugInput()
        {
            // 1键：执行第一阶段测试
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestPhase1();
            }

            // 2键：执行第二阶段测试
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestPhase2();
            }

            // 3键：执行第三阶段测试
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestPhase3();
            }

            // 4键：执行第四阶段测试（存储系统）
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestPhase4();
            }

            // G键：打印格子信息
            if (Input.GetKeyDown(KeyCode.G))
            {
                _gameManager.Debug_PrintGridInfo();
            }

            // P键：打印玩家信息
            if (Input.GetKeyDown(KeyCode.P))
            {
                _gameManager.Debug_PrintPlayerInfo();
            }

            // C键：打印合成信息
            if (Input.GetKeyDown(KeyCode.C))
            {
                _gameManager.Debug_PrintCraftingInfo();
            }

            // E键：获得经验
            if (Input.GetKeyDown(KeyCode.E))
            {
                _gameManager.Debug_GainExperience(50);
            }

            // S键：消耗体力
            if (Input.GetKeyDown(KeyCode.S))
            {
                _gameManager.Debug_UseStamina(1);
            }

            // X键：执行探索
            if (Input.GetKeyDown(KeyCode.X))
            {
                _gameManager.Debug_Explore();
            }

            // U键：显示UI信息
            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug_PrintUIInfo();
            }

            // I键：显示设置面板
            if (Input.GetKeyDown(KeyCode.I))
            {
                ShowSettingsPanel();
            }

            // O键：显示订单面板
            if (Input.GetKeyDown(KeyCode.O))
            {
                ShowOrdersPanel();
            }

            // A键：显示成就面板
            if (Input.GetKeyDown(KeyCode.A))
            {
                ShowAchievementsPanel();
            }
        }

        /// <summary>
        /// 第一阶段测试：框架系统
        /// 验证GridManager、PlayerManager、ItemConfig
        /// </summary>
        private void TestPhase1()
        {
            Debug.Log("\n========== 执行第一阶段测试 ==========");

            var gridManager = _gameManager.GetGridManager();
            var playerManager = _gameManager.GetPlayerManager();

            // 测试1：验证初始网格状态
            Debug.Log("[TestPhase1] 测试1: 验证网格初始化");
            var stats = gridManager.GetStatistics();
            Debug.Log($"  -> 总格子: {stats.totalCellCount}, 锁定: {stats.lockedCellCount}, " +
                     $"空: {stats.emptyCellCount}, 已填: {stats.filledCellCount}");

            // 测试2：验证玩家初始状态
            Debug.Log("[TestPhase1] 测试2: 验证玩家初始化");
            var playerStats = playerManager.GetStatistics();
            Debug.Log($"  -> 等级: {playerStats.level}, 经验: {playerStats.experience}");

            // 测试3：放置物品
            Debug.Log("[TestPhase1] 测试3: 在(3,3)放置净水L1×2");
            _gameManager.Debug_PlaceItem(3, 3, ItemType.Water_L1, 2);

            // 测试4：增加经验验证升级
            Debug.Log("[TestPhase1] 测试4: 获得300经验（验证升级）");
            _gameManager.Debug_GainExperience(300);

            // 测试5：打印完整状态
            Debug.Log("[TestPhase1] 测试5: 打印完整状态");
            _gameManager.Debug_PrintGridInfo();
            _gameManager.Debug_PrintPlayerInfo();

            Debug.Log("========== 第一阶段测试完成 ==========\n");
        }

        /// <summary>
        /// 第二阶段测试：合成系统
        /// 验证CraftingEngine双击合成和拖拽解锁
        /// </summary>
        private void TestPhase2()
        {
            Debug.Log("\n========== 执行第二阶段测试 ==========");

            var gridManager = _gameManager.GetGridManager();
            var playerManager = _gameManager.GetPlayerManager();

            // 测试1：放置合成原料
            Debug.Log("[TestPhase2] 测试1: 放置合成原料");
            _gameManager.Debug_PlaceItem(2, 2, ItemType.Water_L1, 1);
            _gameManager.Debug_PlaceItem(2, 3, ItemType.Water_L1, 1);

            // 测试2：执行双击合成（应该成功）
            Debug.Log("[TestPhase2] 测试2: 在(2,2)执行双击合成 (需要 净水L1×2 → 净水片L2)");
            _gameManager.Debug_DoubleTapCraft(2, 2);

            // 测试3：尝试拖拽解锁
            Debug.Log("[TestPhase2] 测试3: 放置相同物品准备解锁");
            _gameManager.Debug_PlaceItem(5, 5, ItemType.Water_L1, 1);
            _gameManager.Debug_PlaceItem(5, 6, ItemType.Water_L1, 1);

            Debug.Log("[TestPhase2] 测试3: 尝试拖拽解锁 [(5,5) → (4,4)]");
            _gameManager.Debug_DragToUnlock(5, 5, 4, 4);

            // 测试4：检查网格饱和状态
            Debug.Log("[TestPhase2] 测试4: 检查网格合成状态");
            _gameManager.Debug_PrintCraftingInfo();

            // 测试5：查看最终状态
            Debug.Log("[TestPhase2] 测试5: 查看完整状态");
            _gameManager.Debug_PrintGridInfo();
            _gameManager.Debug_PrintPlayerInfo();

            Debug.Log("========== 第二阶段测试完成 ==========\n");
        }

        /// <summary>
        /// 第三阶段测试：探索系统
        /// 验证ExplorationEngine探索和物品放置
        /// </summary>
        private void TestPhase3()
        {
            Debug.Log("\n========== 执行第三阶段测试 ==========");

            var playerManager = _gameManager.GetPlayerManager();
            var gridManager = _gameManager.GetGridManager();
            var orderSystem = _gameManager.GetOrderSystem();
            var orderEngine = _gameManager.GetOrderEngine();

            // 测试1：验证初始体力
            Debug.Log("[TestPhase3] 测试1: 验证初始体力");
            var playerStats = playerManager.GetStatistics();
            Debug.Log($"  -> 当前体力: {playerStats.currentStamina}/{playerStats.maxStamina}");

            // 测试2：进行第一次探索（应该成功）
            Debug.Log("[TestPhase3] 测试2: 执行第一次探索");
            _gameManager.Debug_Explore();

            // 测试3：检查体力消耗
            Debug.Log("[TestPhase3] 测试3: 检查体力消耗");
            playerStats = playerManager.GetStatistics();
            Debug.Log($"  -> 当前体力: {playerStats.currentStamina}/{playerStats.maxStamina}");

            // 测试4：进行第二次探索
            Debug.Log("[TestPhase3] 测试4: 执行第二次探索");
            _gameManager.Debug_Explore();

            // 测试5：检查网格状态
            Debug.Log("[TestPhase3] 测试5: 检查网格状态");
            var gridStats = gridManager.GetStatistics();
            Debug.Log($"  -> 已填: {gridStats.filledCellCount}, 空: {gridStats.emptyCellCount}");

            // 测试6：多次探索直到体力不足
            Debug.Log("[TestPhase3] 测试6: 连续探索直到体力不足");
            for (int i = 0; i < 30; i++)
            {
                if (!_gameManager.GetExplorationEngine().TryExplore())
                {
                    Debug.Log($"[TestPhase3] 第{i+1}次探索失败，停止");
                    break;
                }
            }

            // 测试7：生成每日订单
            Debug.Log("[TestPhase3] 测试7: 生成每日订单");
            _gameManager.Debug_GenerateDailyOrders();

            // 测试8：打印订单信息
            Debug.Log("[TestPhase3] 测试8: 打印订单信息");
            var orders = orderSystem.GetCurrentOrders();
            foreach (var order in orders)
            {
                Debug.Log($"  订单[{order.orderId}]: {order.GetDisplayName()} → {order.rewardType} ×{order.rewardAmount}");
            }

            // 测试9：打印最终状态
            Debug.Log("[TestPhase3] 测试9: 打印最终状态");
            _gameManager.Debug_PrintGridInfo();
            _gameManager.Debug_PrintPlayerInfo();

            Debug.Log("========== 第三阶段测试完成 ==========\n");
        }

        /// <summary>
        /// 第四阶段测试：存储系统验证
        /// 测试保存和加载功能
        /// </summary>
        private void TestPhase4()
        {
            Debug.Log("========== 第四阶段测试：存储系统 ==========");

            // 测试1：打印存储信息
            Debug.Log("[TestPhase4] 测试1: 打印存储信息");
            Debug.Log(_gameManager.GetStorageInfo());

            // 测试2：手动保存数据
            Debug.Log("[TestPhase4] 测试2: 手动保存数据");
            _gameManager.SavePlayerData();
            _gameManager.SaveGridData();

            // 测试3：打印存储信息（确认保存）
            Debug.Log("[TestPhase4] 测试3: 确认保存后的存储信息");
            Debug.Log(_gameManager.GetStorageInfo());

            // 测试4：模拟游戏重启（重新创建GameManager）
            Debug.Log("[TestPhase4] 测试4: 模拟游戏重启");
            var oldPlayerData = _gameManager.GetPlayerData();
            var oldGridStats = _gameManager.GetGridManager().GetStatistics();

            // 销毁旧的GameManager
            Destroy(_gameManager.gameObject);

            // 等待一帧让销毁完成
            StartCoroutine(TestStorageReload(oldPlayerData, oldGridStats));

            Debug.Log("========== 第四阶段测试完成 ==========\n");
        }

        /// <summary>
        /// 测试存储重载的协程
        /// </summary>
        private System.Collections.IEnumerator TestStorageReload(PlayerData oldPlayerData, GridStatistics oldGridStats)
        {
            yield return null; // 等待一帧

            // 创建新的GameManager（会自动加载数据）
            _gameManager = GameManager.Instance;

            yield return new WaitForSeconds(0.1f); // 等待初始化完成

            // 测试5：验证数据重载
            Debug.Log("[TestPhase4] 测试5: 验证数据重载");
            var newPlayerData = _gameManager.GetPlayerData();
            var newGridStats = _gameManager.GetGridManager().GetStatistics();

            bool playerDataMatch = newPlayerData != null &&
                                   newPlayerData.playerName == oldPlayerData.playerName &&
                                   newPlayerData.level == oldPlayerData.level &&
                                   newPlayerData.experience == oldPlayerData.experience;

            bool gridDataMatch = newGridStats.totalCellCount == oldGridStats.totalCellCount &&
                                newGridStats.filledCellCount == oldGridStats.filledCellCount &&
                                newGridStats.lockedCellCount == oldGridStats.lockedCellCount;

            Debug.Log($"  玩家数据匹配: {playerDataMatch}");
            Debug.Log($"  网格数据匹配: {gridDataMatch}");

            if (playerDataMatch && gridDataMatch)
            {
                Debug.Log("[TestPhase4] ✅ 存储系统测试通过！");
            }
            else
            {
                Debug.Log("[TestPhase4] ❌ 存储系统测试失败！");
            }

            // 测试6：清理测试数据（可选）
            // _gameManager.DeleteAllSaveData();
        }

        #region UI调试方法

        /// <summary>
        /// 打印UI系统信息
        /// </summary>
        private void Debug_PrintUIInfo()
        {
            Debug.Log("========== UI系统信息 ==========");
            if (_gameManager != null && _gameManager.GetUIManager() != null)
            {
                var uiManager = _gameManager.GetUIManager();
                Debug.Log($"UI管理器状态: 激活");
                Debug.Log($"网格UI: {(uiManager.GetGridUI() != null ? "已创建" : "未创建")}");
                Debug.Log($"玩家信息面板: {(uiManager.GetPlayerInfoPanel() != null ? "已创建" : "未创建")}");
                Debug.Log($"控制面板: {(uiManager.GetControlPanel() != null ? "已创建" : "未创建")}");
                Debug.Log($"物品详情弹窗: {(uiManager.GetItemDetailPopup() != null ? "已创建" : "未创建")}");
                Debug.Log($"设置面板: {(uiManager.GetSettingsPanel() != null ? "已创建" : "未创建")}");
                Debug.Log($"订单面板: {(uiManager.GetOrdersPanel() != null ? "已创建" : "未创建")}");
            }
            else
            {
                Debug.Log("UI管理器: 未初始化");
            }
            Debug.Log("===============================");
        }

        /// <summary>
        /// 显示设置面板
        /// </summary>
        private void ShowSettingsPanel()
        {
            if (_gameManager != null && _gameManager.GetUIManager() != null)
            {
                var settingsPanel = _gameManager.GetUIManager().GetSettingsPanel();
                if (settingsPanel != null)
                {
                    settingsPanel.Show();
                    Debug.Log("设置面板已显示");
                }
                else
                {
                    Debug.LogWarning("设置面板未创建");
                }
            }
        }

        /// <summary>
        /// 显示订单面板
        /// </summary>
        private void ShowOrdersPanel()
        {
            if (_gameManager != null && _gameManager.GetUIManager() != null)
            {
                var ordersPanel = _gameManager.GetUIManager().GetOrdersPanel();
                if (ordersPanel != null)
                {
                    ordersPanel.Show();
                    Debug.Log("订单面板已显示");
                }
                else
                {
                    Debug.LogWarning("订单面板未创建");
                }
            }
        }

        /// <summary>
        /// 显示成就面板
        /// </summary>
        private void ShowAchievementsPanel()
        {
            if (_gameManager != null && _gameManager.GetUIManager() != null)
            {
                var achievementPanel = _gameManager.GetUIManager().GetAchievementPanel();
                if (achievementPanel != null)
                {
                    achievementPanel.Show();
                    Debug.Log("成就面板已显示");
                }
                else
                {
                    Debug.LogWarning("成就面板未创建");
                }
            }
        }

        #endregion
    }
}
