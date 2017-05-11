using System;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Item;

namespace CScape.Basic.Model
{
    public class BasicItem : IItemDefinition
    {
        public bool Equals(IItemDefinition other)
        {
            if (other == null) return false;
            if (Object.ReferenceEquals(this, other)) return true;
            return ItemId == other.ItemId;
        }

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

        public void UseWith(Player user, IItemDefinition other)
        {
            throw new NotImplementedException();
        }
    }
}