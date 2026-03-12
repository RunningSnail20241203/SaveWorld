using UnityEngine;
using TestWebGL.Game.Core;
using TestWebGL.Game.Items;

namespace TestWebGL.Game
{
    /// <summary>
    /// 游戏启动入口
    /// 挂载到SampleScene中，用于初始化游戏
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        private void Awake()
        {
            // 获取GameManager实例，触发初始化
            var gameManager = GameManager.Instance;
            Debug.Log("[GameEntry] 游戏启动!");
        }

        private void Start()
        {
            // 第一阶段测试代码
            if (Input.GetKeyDown(KeyCode.D))
            {
                TestPhase1();
            }
        }

        private void Update()
        {
            HandleDebugInput();
        }

        /// <summary>
        /// 第一阶段功能测试
        /// </summary>
        private void TestPhase1()
        {
            Debug.Log("\n========== 第一阶段测试 ==========\n");

            var gameManager = GameManager.Instance;

            // 测试1: 输出格子信息
            Debug.Log("【测试1】格子系统初始化");
            gameManager.Debug_PrintGridInfo();

            // 测试2: 输出玩家信息
            Debug.Log("\n【测试2】玩家系统初始化");
            gameManager.Debug_PrintPlayerInfo();

            // 测试3: 在中心3×3区域放置一些L1物品
            Debug.Log("\n【测试3】在解锁区域放置L1物品");
            gameManager.Debug_PlaceItem(3, 3, ItemType.Water_L1, 5);
            gameManager.Debug_PlaceItem(3, 4, ItemType.Food_L1, 3);
            gameManager.Debug_PlaceItem(4, 4, ItemType.Tool_L1, 2);
            gameManager.Debug_PlaceItem(5, 4, ItemType.Home_L1, 1);

            gameManager.Debug_PrintGridInfo();

            // 测试4: 测试玩家经验获得和升级
            Debug.Log("\n【测试4】测试玩家升级机制");
            gameManager.Debug_GainExperience(250);  // 达到Lv2
            gameManager.Debug_PrintPlayerInfo();

            // 测试5: 测试体力消耗
            Debug.Log("\n【测试5】测试体力系统");
            for (int i = 0; i < 5; i++)
            {
                gameManager.Debug_UseStamina(1);
            }
            gameManager.Debug_PrintPlayerInfo();

            Debug.Log("\n========== 测试完成 ==========\n");
        }

        /// <summary>
        /// 第二阶段功能测试 - 合成系统
        /// </summary>
        private void TestPhase2()
        {
            Debug.Log("\n========== 第二阶段测试（合成系统）==========\n");

            var gameManager = GameManager.Instance;

            // 测试1: 准备合成环境
            Debug.Log("【测试1】准备合成环境 - 放置2个净水");
            gameManager.Debug_PlaceItem(3, 3, ItemType.Water_L1, 2);
            gameManager.Debug_PlaceItem(3, 4, ItemType.Food_L1, 4);
            gameManager.Debug_PlaceItem(4, 4, ItemType.Tool_L1, 3);
            gameManager.Debug_PrintGridInfo();

            // 测试2: 双击合成
            Debug.Log("\n【测试2】双击格子(3,3)进行合成");
            gameManager.Debug_DoubleTapCraft(3, 3);
            gameManager.Debug_PrintGridInfo();

            // 测试3: 再次合成
            Debug.Log("\n【测试3】继续在格子(3,4)合成食物");
            gameManager.Debug_DoubleTapCraft(3, 4);
            gameManager.Debug_PrintGridInfo();

            // 测试4: 拖拽解锁
            Debug.Log("\n【测试4】拖拽解锁 - 将工具×2拖到[0,0]解锁");
            gameManager.Debug_DragToUnlock(4, 4, 0, 0);
            gameManager.Debug_PrintGridInfo();

            // 测试5: 输出合成状态
            Debug.Log("\n【测试5】合成系统状态");
            gameManager.Debug_PrintCraftingInfo();

            gameManager.Debug_PrintPlayerInfo();

            Debug.Log("\n========== 测试完成 ==========\n");
        }

        /// <summary>
        /// 处理调试输入
        /// </summary>
        private void HandleDebugInput()
        {
            var gameManager = GameManager.Instance;

            // D键: 触发第一阶段测试
            if (Input.GetKeyDown(KeyCode.D))
            {
                TestPhase1();
            }

            // 2键: 触发第二阶段测试
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestPhase2();
            }

            // G键: 输出格子信息
            if (Input.GetKeyDown(KeyCode.G))
            {
                gameManager.Debug_PrintGridInfo();
            }

            // P键: 输出玩家信息
            if (Input.GetKeyDown(KeyCode.P))
            {
                gameManager.Debug_PrintPlayerInfo();
            }

            // C键: 输出合成系统信息
            if (Input.GetKeyDown(KeyCode.C))
            {
                gameManager.Debug_PrintCraftingInfo();
            }

            // E键: 获得100经验
            if (Input.GetKeyDown(KeyCode.E))
            {
                gameManager.Debug_GainExperience(100);
            }

            // S键: 消耗1体力
            if (Input.GetKeyDown(KeyCode.S))
            {
                gameManager.Debug_UseStamina(1);
            }
        }
    }
}
