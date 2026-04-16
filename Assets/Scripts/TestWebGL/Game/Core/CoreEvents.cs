using System;

namespace SaveWorld.Game.Core
{
    #region 游戏事件

    public class ItemDragStartEvent : GameEvent
    {
        public int CellId { get; }

        public ItemDragStartEvent(int cellId)
        {
            CellId = cellId;
        }
    }

    public class ItemDropEvent : GameEvent
    {
        public int SourceCellId { get; }
        public int TargetCellId { get; }

        public ItemDropEvent(int sourceCellId, int targetCellId)
        {
            SourceCellId = sourceCellId;
            TargetCellId = targetCellId;
        }
    }

    public class MergeRequestEvent : GameEvent
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

    public class ExplorationRequestEvent : GameEvent
    {
        public int StaminaCost { get; set; } = 5;
    }

    public class MergeCompleteEvent : GameEvent
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

    public class ItemMovedEvent : GameEvent
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

    public class ItemSwappedEvent : GameEvent
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

    public class ExplorationCompleteEvent : GameEvent
    {
        public int[] GeneratedCellIds { get; }
        public int StaminaUsed { get; }

        public ExplorationCompleteEvent(int[] generatedCellIds, int staminaUsed)
        {
            GeneratedCellIds = generatedCellIds;
            StaminaUsed = staminaUsed;
        }
    }

    public class OrderSubmittedEvent : GameEvent
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

    public class LevelUpEvent : GameEvent
    {
        public int OldLevel { get; }
        public int NewLevel { get; }

        public LevelUpEvent(int oldLevel, int newLevel)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }

    public class ItemCraftedEvent : GameEvent
    {
        public ItemType ItemType { get; }

        public ItemCraftedEvent(ItemType itemType)
        {
            ItemType = itemType;
        }
    }

    public class ExperienceGainedEvent : GameEvent
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
