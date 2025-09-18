using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld
{
    // 出牌命令
    public class PlayCardCommand : IGameCommand
    {
        private readonly int _playerId;
        private readonly int _cardIndex;
        private readonly int _terrainIndex;
        private Card _playedCard;
        private int? _previousEnergy;
    
        public PlayCardCommand(int playerId, int cardIndex, int terrainIndex)
        {
            _playerId = playerId;
            _cardIndex = cardIndex;
            _terrainIndex = terrainIndex;
        }
    
        public IEnumerator Execute(GameState gameState)
        {
            var player = gameState.Players[_playerId];
            _playedCard = player.Hand[_cardIndex];
        
            if (player.Energy >= _playedCard.Cost)
            {
                _previousEnergy = player.Energy;
                player.Energy -= _playedCard.Cost;
                player.Hand.RemoveAt(_cardIndex);
                gameState.Terrains[_terrainIndex].AddCard(_playedCard, _playerId);
                _playedCard.OnPlay(gameState, _terrainIndex);
            }
            yield break;
        }
    
        public IEnumerator Undo(GameState gameState)
        {
            if (_playedCard != null && _previousEnergy.HasValue)
            {
                var player = gameState.Players[_playerId];
                var terrain = gameState.Terrains[_terrainIndex];
            
                terrain.RemoveCard(_playedCard, _playerId);
                _playedCard.OnUndo(gameState, _terrainIndex);
                player.Hand.Insert(_cardIndex, _playedCard);
                player.Energy = _previousEnergy.Value;
            }
            yield break;
        }
    }

}