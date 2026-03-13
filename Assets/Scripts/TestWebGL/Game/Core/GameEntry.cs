using UnityEngine;
using TestWebGL.Game.Items;
using TestWebGL.Game.Grid;

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
            // D键：执行第一阶段测试
            if (Input.GetKeyDown(KeyCode.D))
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
    }
}
