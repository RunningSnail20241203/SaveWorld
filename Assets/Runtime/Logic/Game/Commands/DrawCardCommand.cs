using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld
{
    // 抽牌命令
    public class DrawCardCommand : IGameCommand
    {
        private readonly int _playerId;
        private Card _drawnCard;
    
        public DrawCardCommand(int playerId)
        {
            _playerId = playerId;
        }
    
        public IEnumerator Execute(GameState gameState)
        {
            var player = gameState.Players[_playerId];
            if (player.Deck.Count > 0)
            {
                _drawnCard = player.Deck[0];
                player.Deck.RemoveAt(0);
                player.Hand.Add(_drawnCard);
            }
            yield break;
        }
    
        public IEnumerator Undo(GameState gameState)
        {
            if (_drawnCard != null)
            {
                var player = gameState.Players[_playerId];
                player.Hand.Remove(_drawnCard);
                player.Deck.Insert(0, _drawnCard);
                _drawnCard = null;
            }
            yield break;
        }
    }
}