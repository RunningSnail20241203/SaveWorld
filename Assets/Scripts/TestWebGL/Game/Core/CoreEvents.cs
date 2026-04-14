using System;
using TestWebGL.Game.Items;

namespace SaveWorld.Game.Core
{
    #region 游戏事件

    public class ItemDragStartEvent : IGameEvent
    {
        public int CellId { get; }

        public ItemDragStartEvent(int cellId)
        {
            CellId = cellId;
        }
    }

    public class ItemDropEvent : IGameEvent
    {
        public int SourceCellId { get; }
        public int TargetCellId { get; }

        public ItemDropEvent(int sourceCellId, int targetCellId)
        {
            SourceCellId = sourceCellId;
            TargetCellId = targetCellId;
        }
    }

    public class MergeRequestEvent : IGameEvent
    {
        public int CellIdA { get; }
        public int CellIdB { get; }
        public int ResultItemId { get; set; }
        public float RewardMultiplier { get; set; } = 1.0f;

        public MergeRequestEvent(int cellIdA, int cellIdB)
        {
            CellIdA = cellIdA;
            CellIdB = cellIdB;
        }
    }

    public class ExplorationRequestEvent : IGameEvent
    {
        public int StaminaCost { get; set; } = 5;
    }

    public class MergeCompleteEvent : IGameEvent
    {
        public int CellId { get; }
        public int OldItemId { get; }
        public int NewItemId { get; }
        public float RewardMultiplier { get; }

        public MergeCompleteEvent(int cellId, int oldItemId, int newItemId, float rewardMultiplier)
        {
            CellId = cellId;
            OldItemId = oldItemId;
            NewItemId = newItemId;
            RewardMultiplier = rewardMultiplier;
        }
    }

    public class ItemMovedEvent : IGameEvent
    {
        public int FromCellId { get; }
        public int ToCellId { get; }
        public int ItemId { get; }

        public ItemMovedEvent(int fromCellId, int toCellId, int itemId)
        {
            FromCellId = fromCellId;
            ToCellId = toCellId;
            ItemId = itemId;
        }
    }

    public class ItemSwappedEvent : IGameEvent
    {
        public int CellIdA { get; }
        public int CellIdB { get; }
        public int ItemIdA { get; }
        public int ItemIdB { get; }

        public ItemSwappedEvent(int cellIdA, int cellIdB, int itemIdA, int itemIdB)
        {
            CellIdA = cellIdA;
            CellIdB = cellIdB;
            ItemIdA = itemIdA;
            ItemIdB = itemIdB;
        }
    }

    public class ExplorationCompleteEvent : IGameEvent
    {
        public int[] GeneratedCellIds { get; }
        public int StaminaUsed { get; }

        public ExplorationCompleteEvent(int[] generatedCellIds, int staminaUsed)
        {
            GeneratedCellIds = generatedCellIds;
            StaminaUsed = staminaUsed;
        }
    }

    public class OrderSubmittedEvent : IGameEvent
    {
        public int OrderId { get; }
        public long RewardGold { get; }
        public long RewardExp { get; }

        public OrderSubmittedEvent(int orderId, long rewardGold, long rewardExp)
        {
            OrderId = orderId;
            RewardGold = rewardGold;
            RewardExp = rewardExp;
        }
    }

    public class LevelUpEvent : IGameEvent
    {
        public int OldLevel { get; }
        public int NewLevel { get; }

        public LevelUpEvent(int oldLevel, int newLevel)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }

    public class ItemCraftedEvent : IGameEvent
    {
        public ItemType ItemType { get; }

        public ItemCraftedEvent(ItemType itemType)
        {
            ItemType = itemType;
        }
    }

    public class ExperienceGainedEvent : IGameEvent
    {
        public int Amount { get; }
        public string Source { get; }

        public ExperienceGainedEvent(int amount, string source)
        {
            Amount = amount;
            Source = source;
        }
    }

    #endregion
}