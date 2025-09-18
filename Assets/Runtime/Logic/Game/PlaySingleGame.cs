using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityGameFramework.Runtime;

namespace SaveWorld
{
    public class PlaySingleGame : GameBase
    {
        private float m_ElapseSeconds;

        public override GameMode GameMode => GameMode.PlaySingle;

        public override void InitializeGame()
        {
            _gameState = new GameState();

            // 初始化玩家
            for (int i = 0; i < 2; i++)
            {
                var player = _gameState.Players[i];
                player.Energy = 2;
                player.Deck = CreateDeck(); // 创建12张牌的卡组
                player.Hand = player.Deck.Take(5).ToList(); // 初始5张手牌
                player.Deck = player.Deck.Skip(5).ToList();
            }

            // 设置地形加成
            _gameState.Terrains[0].Modifier = 1.0f; // 普通地形
            _gameState.Terrains[1].Modifier = 1.2f; // 加成地形
            _gameState.Terrains[2].Modifier = 0.8f; // 减益地形
        }

        public override IEnumerator GameLoop()
        {
            while (_gameState.TurnCount <= 6)
            {
                var currentPlayer = _gameState.Players[_gameState.CurrentPlayer];

                // 回合开始抽牌(第一回合不抽)
                if (_gameState.TurnCount > 1)
                {
                    var drawCmd = new DrawCardCommand(_gameState.CurrentPlayer);
                    yield return  _gameState.ExecuteCommand(drawCmd);
                }

                // 玩家/AI行动阶段
                bool turnEnded = false;
                while (!turnEnded)
                {
                    if (currentPlayer.IsAI)
                    {
                        // AI逻辑生成命令
                        var aiCommand = GenerateAICommand();
                        yield return _gameState.ExecuteCommand(aiCommand);

                        // AI决定结束回合
                        if (ShouldAIEndTurn())
                        {
                            var endCmd = new EndTurnCommand(_gameState.CurrentPlayer);
                            yield return _gameState.ExecuteCommand(endCmd);
                            turnEnded = true;
                        }
                    }
                    else
                    {
                        // 显示游戏状态
                        DisplayGameState();

                        // 获取玩家输入
                        var action = GetPlayerAction();

                        switch (action.ActionType)
                        {
                            case PlayerActionType.PlayCard:
                                var playCmd = new PlayCardCommand(
                                    _gameState.CurrentPlayer,
                                    action.CardIndex,
                                    action.TerrainIndex
                                );
                                yield return  _gameState.ExecuteCommand(playCmd);
                                break;

                            case PlayerActionType.EndTurn:
                                var endCmd = new EndTurnCommand(_gameState.CurrentPlayer);
                                yield return  _gameState.ExecuteCommand(endCmd);
                                turnEnded = true;
                                break;

                            case PlayerActionType.Undo:
                                yield return  _gameState.UndoLastCommand();
                                break;
                        }
                    }
                }
            }

            // 游戏结束，计算胜负
            yield return DetermineWinner();
        }
        
        private IGameCommand GenerateAICommand()
        {
            // 简单的AI逻辑
            var currentPlayer = _gameState.Players[_gameState.CurrentPlayer];
            var compositeCmd = new CompositeCommand();

            // 如果有足够能量，尝试出牌
            if (currentPlayer.Hand.Count > 0 && currentPlayer.Energy > 0)
            {
                // 找出能出且最强的牌
                var playableCards = currentPlayer.Hand
                    .Select((card, index) => new { Card = card, Index = index })
                    .Where(x => x.Card.Cost <= currentPlayer.Energy)
                    .OrderByDescending(x => x.Card.Power)
                    .ToList();

                if (playableCards.Count > 0)
                {
                    var bestCard = playableCards.First();
                    var terrainIndex = ChooseBestTerrain(bestCard.Card);
                    compositeCmd.AddCommand(new PlayCardCommand(
                        _gameState.CurrentPlayer,
                        bestCard.Index,
                        terrainIndex
                    ));
                }
            }

            return compositeCmd;
        }
        
        private int ChooseBestTerrain(Card card)
        {
            // 简单选择加成最高的地形
            return _gameState.Terrains
                .Select((terrain, index) => new { Terrain = terrain, Index = index })
                .OrderByDescending(x => x.Terrain.Modifier)
                .First()
                .Index;
        }

        private bool ShouldAIEndTurn()
        {
            // 简单逻辑：没有能量或没有牌可出时结束回合
            var currentPlayer = _gameState.Players[_gameState.CurrentPlayer];
            return currentPlayer.Energy == 0 ||
                   !currentPlayer.Hand.Any(card => card.Cost <= currentPlayer.Energy);
        }

        private IEnumerator DetermineWinner()
        {
            int player1Wins = 0;
            int player2Wins = 0;

            foreach (var terrain in _gameState.Terrains)
            {
                var p1Power = terrain.CalculatePower(0);
                var p2Power = terrain.CalculatePower(1);

                if (p1Power > p2Power) player1Wins++;
                else if (p2Power > p1Power) player2Wins++;
            }

            Log.Info($"游戏结束! 玩家1赢得 {player1Wins} 个地形，玩家2赢得 {player2Wins} 个地形");
            if (player1Wins > player2Wins) Log.Info("玩家1获胜!");
            else if (player2Wins > player1Wins) Log.Info("玩家2获胜!");
            else Log.Info("平局!");
            yield break;
        }

        // 其他辅助方法...
        private void DisplayGameState()
        {
            /* ... */
            throw new NotImplementedException();
        }

        private PlayerAction GetPlayerAction()
        {
            /* ... */
            throw new NotImplementedException();
        }
        
        protected virtual List<Card> CreateDeck()
        {
            throw new NotImplementedException();
        }
    }
}
