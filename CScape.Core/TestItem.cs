using CScape.Core.Extensions;
using CScape.Models.Game.Combat;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Item;

namespace CScape.Basic.Model
{
    // todo : proper item model
    public class TestItem : IEquippableItem
    {
        public int ItemId { get; }
        public string Name { get; }
        public int MaxAmount { get; }
        public bool IsTradable { get; }
        public float Weight { get; }
        public bool IsNoted { get; }
        public int NoteSwitchId { get; }



        public TestItem(int itemId, string name, int maxAmount, bool isTradable, float weight, bool isNoted, int noteSwitchId)
        {
            ItemId = itemId;
            Name = name;
            MaxAmount = maxAmount;
            IsTradable = isTradable;
            Weight = weight;
            IsNoted = isNoted;
            NoteSwitchId = noteSwitchId;
        }

        public bool Equals(IItemDefinition other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ItemId == other.ItemId;
        }

        public EquipSlotType Slot { get; } = EquipSlotType.Head;
        public IEquipmentStats Attack { get; } = NullEquipmentStats.Instance;
        public IEquipmentStats Defence { get; } = NullEquipmentStats.Instance;
        public int StrengthBonus { get; } = 1;
        public int MagicBonus { get; } = 2;
        public int RangedBonus { get; } = 3;
        public int PrayerBonus { get; } = 4;
        public IWeaponCombatType CombatType { get; } = NullWeaponCombatType.Instance;
         
        public void UseWith(IEntity entity, ItemStack other)
        {
            entity.SystemMessage($"Use {Name}:{ItemId} with {other.Id.Name}{other.Id.ItemId}");
        }

        public void OnAction(IEntity entity, int actionId)
        {
            entity.SystemMessage($"On action {Name}:{ItemId} action: {actionId}");
        }

        public bool CanEquip(IEntity entity) => true;
        
        public void OnEquip(IEntity entity)
        {
            entity.SystemMessage($"Equipped {Name}:{ItemId}");
        }

    }
}