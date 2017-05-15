using System;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;

namespace CScape.Basic.Model
{
    public class BasicItem : IItemDefinition
    {
        public int ItemId { get; }
        public string Name { get; }
        public int MaxAmount { get; }
        public bool IsTradable { get; }
        public float Weight { get; }
        public bool IsNoted { get; }
        public int NoteSwitchId { get; }

        public BasicItem(int itemId, string name, int maxAmount, bool isTradable, float weight, bool isNoted, int noteSwitchId)
        {
            ItemId = itemId;
            Name = name;
            MaxAmount = maxAmount;
            IsTradable = isTradable;
            Weight = weight;
            IsNoted = isNoted;
            NoteSwitchId = noteSwitchId;
        }

        public void UseWith(Player player, IItemManager manager, int ourIdx, IItemDefinition otherItem, int otherIndex)
        {
            player.DebugMsg($"Use [i:{ItemId}x{manager.Provider.Amounts[ourIdx]}] with [i:{otherItem}x{manager.Provider.Amounts[ourIdx]}]", ref player.DebugItems);
        }

        public void OnAction(Player player, IItemManager manager, int index, ItemActionType type)
        {
            player.DebugMsg($"Action {type} on [i:{ItemId}x{manager.Provider.Amounts[index]}] ", ref player.DebugItems);
        }

        public bool Equals(IItemDefinition other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ItemId == other.ItemId;
        }
    }
}