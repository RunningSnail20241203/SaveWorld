using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld
{
    // 命令接口
    public interface IGameCommand
    {
        IEnumerator Execute(GameState gameState);
        IEnumerator Undo(GameState gameState);
    }
}