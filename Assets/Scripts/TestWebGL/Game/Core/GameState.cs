using System;
using System.Collections.Generic;

namespace SaveWorld.Game.Core
{
    /// <summary>
    /// 第一层: 状态层
    /// 纯数据对象，不可变，没有任何逻辑
    /// 所有修改只能通过事件进行
    /// </summary>
    public sealed class GameState
    {
        public readonly int Version;
        
        // 格子状态 7列 x 9行 = 63格
        public readonly CellState[] Cells;
        
        // 玩家状态
        public readonly PlayerState Player;
        
        // 元数据扩展字典，所有机制可以在这里存放自己的数据
        public readonly IReadOnlyDictionary<string, object> Metadata;

        internal GameState(int version, CellState[] cells, PlayerState player, IReadOnlyDictionary<string, object> metadata)
        {
            Version = version;
            Cells = cells;
            Player = player;
            Metadata = metadata;
        }

        public static GameState CreateInitial()
        {
            var cells = new CellState[63];
            for (int i = 0; i < 63; i++)
            {
                cells[i] = CellState.Empty(i);
            }

            return new GameState(
                version: 0,
                cells: cells,
                player: PlayerState.CreateInitial(),
                metadata: new Dictionary<string, object>()
            );
        }
    }

    /// <summary>
    /// 单个格子的状态
    /// 纯值对象，不可变
    /// </summary>
    public readonly struct CellState
    {
        public readonly int Index;
        public readonly int ItemId;
        public readonly int Count;
        public readonly bool IsLocked;

        private CellState(int index, int itemId, int count, bool isLocked)
        {
            Index = index;
            ItemId = itemId;
            Count = count;
            IsLocked = isLocked;
        }

        public static CellState Empty(int index) => new CellState(index, 0, 0, false);
        
        public static CellState Create(int index, int itemId, int count) => new CellState(index, itemId, count, false);

        public bool IsEmpty() => ItemId == 0;
    }

    /// <summary>
    /// 玩家状态
    /// 纯值对象，不可变
    /// </summary>
    public readonly struct PlayerState
    {
        public readonly int Level;
        public readonly int Stamina;
        public readonly long Gold;
        public readonly long LastOfflineTime;

        private PlayerState(int level, int stamina, long gold, long lastOfflineTime)
        {
            Level = level;
            Stamina = stamina;
            Gold = gold;
            LastOfflineTime = lastOfflineTime;
        }

        public static PlayerState CreateInitial()
        {
            return new PlayerState(
                level: 1,
                stamina: 20,
                gold: 0,
                lastOfflineTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            );
        }
    }
}