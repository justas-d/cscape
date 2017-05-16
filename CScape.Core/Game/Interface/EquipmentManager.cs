using System;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Item;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    /// <summary>
    /// Defines an item manager that manages standard item equipment and stats for characters.
    /// Provider should be at least 14 slots in size. Any extra slots will be ignored.
    /// </summary>
    public class EquipmentManager : AbstractSyncedItemManager
    {
        public const int EquipmentMaxSize = 14;

        [NotNull] private readonly Player _player;
        private readonly IItemDefinitionDatabase _db;

        public EquipmentManager(int interfaceId, [NotNull] Player player, [NotNull] IServiceProvider services, [NotNull] IItemProvider provider)
            : base(interfaceId, provider)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _db = services.ThrowOrGet<IItemDefinitionDatabase>();

            if(Size > EquipmentMaxSize)
                throw new ArgumentOutOfRangeException(nameof(Size));
        }

        public override ItemProviderChangeInfo CalcChangeInfo(int id, int deltaAmount)
        {
            // get def
            var def = _db.GetAsserted(id) as IEquippableItem;

            // only add equippables
            if (def == null)
                return ItemProviderChangeInfo.Invalid;

            // validate idx
            var (isValid, idx) = IsValidIdx(def.Slot);
            if (!isValid)
                return ItemProviderChangeInfo.Invalid;

            if (Provider.IsEmptyAtIndex(idx))
            {
                // disallow remove operations
                if (deltaAmount < 0)
                    return ItemProviderChangeInfo.Invalid;

                // try equipping.
                if (!def.CanEquip(_player))
                    return ItemProviderChangeInfo.Invalid;

                // can equip and slot is empty, return info.
                return new ItemProviderChangeInfo(idx, deltaAmount, 0, id);
            }

            // item exists, only allow valid info if id's match
            if (Provider.GetId(idx) == id)
            {
                // handle overflow
                long uncheckedAmount = Provider.GetAmount(idx) + deltaAmount;
                var overflow = ItemHelper.CalculateOverflow(def, uncheckedAmount);

                return new ItemProviderChangeInfo(idx, Convert.ToInt32(uncheckedAmount - overflow), overflow, id);

            }

            return ItemProviderChangeInfo.Invalid;
        }

        protected override bool InternalExecuteChangeInfo(ItemProviderChangeInfo info)
        {
            var success = ItemHelper.ExecuteChangeInfo(this, info);
            if (!success)
                return false;

            var def = _db.GetAsserted(info.NewItemDefId) as IEquippableItem;

            // call on equip on def.
            def?.OnEquip(_player, this, info.Index);
            return true;
        }

        private (bool status, int val) IsValidIdx(EquipSlotType slot)
        {
            var idx = (int) slot;

            if (0 > idx || idx >= Size)
                return (false, -1);

            return (true, idx);
        }

        public override int Contains(int id)
        {
            var def = _db.GetAsserted(id) as IEquippableItem;
            if (def == null)
                return 0;

            // validate idx
            var (isValid, idx) = IsValidIdx(def.Slot);
            if (!isValid)
                return 0;

            // return amount in slot
            return Provider.GetAmount(idx);
        }
    }
}