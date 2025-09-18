using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveWorld
{
    // 复合命令
    public class CompositeCommand : IGameCommand
    {
        private readonly List<IGameCommand> _commands = new List<IGameCommand>();

        public void AddCommand(IGameCommand command)
        {
            _commands.Add(command);
        }

        public IEnumerator Execute(GameState gameState)
        {
            foreach (var cmd in _commands)
            {
                yield return cmd.Execute(gameState);
            }
        }

        public IEnumerator Undo(GameState gameState)
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                yield return _commands[i].Undo(gameState);
            }
        }
    }
}