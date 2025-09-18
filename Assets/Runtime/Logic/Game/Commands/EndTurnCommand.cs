using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld
{
    // 回合结束命令
    public class EndTurnCommand : IGameCommand
    {
        private readonly int _playerId;
        private int? _nextPlayerId;
        private bool _energyAdded;
    
        public EndTurnCommand(int playerId)
        {
            _playerId = playerId;
        }
    
        public IEnumerator Execute(GameState gameState)
        {
            _nextPlayerId = 1 - _playerId; // 切换玩家
            gameState.CurrentPlayer = _nextPlayerId.Value;
        
            // 下回合玩家增加能量
            var nextPlayer = gameState.Players[_nextPlayerId.Value];
            nextPlayer.Energy += 1;
            _energyAdded = true;
        
            // 回合计数
            if (_nextPlayerId == 0)
            {
                gameState.TurnCount += 1;
            }
            
            yield break;
        }
    
        public IEnumerator Undo(GameState gameState)
        {
            if (_energyAdded && _nextPlayerId.HasValue)
            {
                var nextPlayer = gameState.Players[_nextPlayerId.Value];
                nextPlayer.Energy -= 1;
                _energyAdded = false;
            }
        
            gameState.CurrentPlayer = _playerId;
        
            if (_nextPlayerId == 0)
            {
                gameState.TurnCount -= 1;
            }
            
            yield break;
        }
    }
}