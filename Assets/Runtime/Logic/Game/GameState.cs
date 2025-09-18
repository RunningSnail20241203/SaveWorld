using System.Collections;
using System.Collections.Generic;

namespace SaveWorld
{
    // 游戏状态
    public class GameState
    {
        public Player[] Players { get; } = new Player[2];
        public Terrain[] Terrains { get; } = new Terrain[3];
        public int CurrentPlayer { get; set; }
        public int TurnCount { get; set; } = 1;
        public Stack<IGameCommand> CommandHistory { get; } = new();

        public GameState()
        {
            Players[0] = new Player();
            Players[1] = new Player();
            for (int i = 0; i < 3; i++)
            {
                Terrains[i] = new Terrain();
            }
        }

        public IEnumerator ExecuteCommand(IGameCommand command)
        {
            yield return command.Execute(this);
            CommandHistory.Push(command);
        }

        public IEnumerator UndoLastCommand()
        {
            if (CommandHistory.Count > 0)
            {
                var command = CommandHistory.Pop();
                yield return command.Undo(this);
            }
        }
    }
}