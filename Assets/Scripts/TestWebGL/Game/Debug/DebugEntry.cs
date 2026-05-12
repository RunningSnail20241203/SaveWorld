using UnityEngine;
using UnityEngine.InputSystem;
using SaveWorld.Game.Core;

namespace SaveWorld.Game.EditorDebug
{
    /// <summary>
    /// 游戏入口脚本
    /// 挂载在场景中，负责初始化游戏和处理调试输入
    /// </summary>
    public class DebugEntry : MonoBehaviour
    {
        private GameLoop _gameLoop;

        private void Awake()
        {
        }

        private void Start()
        {
            _gameLoop = GameLoop.Instance;
            Debug.Log("✅ 游戏初始化完成");
        }

        private void Update()
        {
            HandleDebugInput();
        }

        private void HandleDebugInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // G键：打印格子信息
            if (keyboard.gKey.wasPressedThisFrame)
            {
                Debug_PrintGridInfo();
            }

            // P键：打印玩家信息
            if (keyboard.pKey.wasPressedThisFrame)
            {
                Debug_PrintPlayerInfo();
            }

            // E键：测试事件
            if (keyboard.eKey.wasPressedThisFrame)
            {
                _gameLoop.EventBus.Dispatch(new MergeCompleteEvent(0, 1, 2, 1.0f));
            }

            // X键：执行探索
            if (keyboard.xKey.wasPressedThisFrame)
            {
                _gameLoop.EventBus.Dispatch(new ExplorationRequestEvent());
            }
        }

        private void Debug_PrintGridInfo()
        {
            Debug.Log("========== 格子状态 ==========");
            int filled = 0;
            for (int i = 0; i < 63; i++)
            {
                if (!_gameLoop.CurrentState.Cells[i].IsEmpty())
                {
                    filled++;
                    Debug.Log($"格子[{i}]: 物品ID {_gameLoop.CurrentState.Cells[i].ItemId} × {_gameLoop.CurrentState.Cells[i].Count}");
                }
            }
            Debug.Log($"总格子:63 已填充:{filled} 空:{63-filled}");
        }

        private void Debug_PrintPlayerInfo()
        {
            Debug.Log("========== 玩家状态 ==========");
            Debug.Log($"等级: {_gameLoop.CurrentState.Player.Level}");
            Debug.Log($"体力: {_gameLoop.CurrentState.Player.Stamina}");
            Debug.Log($"金币: {_gameLoop.CurrentState.Player.Gold}");
        }
    }
}