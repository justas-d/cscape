using System;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Item;
using CScape.Core.Injection;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    /// <summary>
    /// Defines an item manager that manages standard item equipment and stats for characters.
    /// Provider should be at least 14 slots in size. Any extra slots will be ignored.
    /// </summary>
    
    public class EquipmentManager : AbstractSyncedItemManager
    {
        /*
        public const int EquipmentMaxSize = 14;

        public EquipmentStats Stats { get; }

        [NotNull] private readonly Player _player;
        private readonly IItemDefinitionDatabase _db;
        
        public EquipmentManager(
            int interfaceId, [NotNull] Player player, [NotNull] IServiceProvider services,
            [NotNull] IItemProvider provider)
            : base(interfaceId, provider)
        {
            if (Size > EquipmentMaxSize)
                throw new ArgumentOutOfRangeException(nameof(Size));

            _player = player ?? throw new ArgumentNullException(nameof(player));
            _db = services.ThrowOrGet<IItemDefinitionDatabase>();

            Stats = new EquipmentStats(services);
            Stats.Update(this);
            SyncStats();
        }

        public override ItemChangeInfo CalcChangeInfo(int id, int deltaAmount)
        {
            if(deltaAmount == 0)
                return ItemChangeInfo.Invalid;

            // get def
            var def = _db.GetAsserted(id) as IEquippableItem;

            // only add equipables
            if (def == null)
                return ItemChangeInfo.Invalid;

            // validate idx
            var (isValid, idx) = IsValidIdx(def.Slot);
            if (!isValid)
                return ItemChangeInfo.Invalid;

            if (Provider.IsEmptyAtIndex(idx))
            {
                // disallow remove operations
                if (deltaAmount < 0)
                    return ItemChangeInfo.Invalid;

                // try equipping.
                if (!def.CanEquip(_player))
                    return ItemChangeInfo.Invalid;

                // can equip and slot is empty, return info.
                return new ItemChangeInfo(idx, deltaAmount, 0, id);
            }

            // item exists, only allow valid info if id's match
            if (Provider.GetId(idx) == id)
            {
                // handle overflow
                long uncheckedAmount = Provider.GetAmount(idx) + deltaAmount;
                var overflow = ItemHelper.CalculateOverflow(def, uncheckedAmount);

                return new ItemChangeInfo(idx, Convert.ToInt32(uncheckedAmount - overflow), overflow, id);

            }

            return ItemChangeInfo.Invalid;
        }
        */

        protected override bool InternalExecuteChangeInfo(ItemChangeInfo info)
        {
            /*
            var success = ItemHelper.ExecuteChangeInfo(this, info);
            if (!success)
                return false;

            var def = _db.GetAsserted(info.NewItemDefId) as IEquippableItem;
            */
            // keep stats updated
            Stats.Update(this);
            SyncStats();

            // call on equip on def.
            def?.OnEquip(_player, this, info.Index);
            //return true;
        }

        /*
        private (bool status, int val) IsValidIdx(EquipSlotType slot)
        {
            var idx = (int) slot;

            if (0 > idx || idx >= Size)
                return (false, -1);

            return (true, idx);
        }
        */

        private void SyncStats()
        {
            string Format(int num) => num >= 0 ? $"+{num}" : num.ToString();

            // todo : abstract this into a meta packet
            PushUpdate(new SetInterfaceTextPacket(1675, $"Stab: {Format(Stats.Attack.Stab)}"));
            PushUpdate(new SetInterfaceTextPacket(1676, $"Slash: {Format(Stats.Attack.Slash)}"));
            PushUpdate(new SetInterfaceTextPacket(1677, $"Crush: {Format(Stats.Attack.Crush)}"));
            PushUpdate(new SetInterfaceTextPacket(1678, $"Magic: {Format(Stats.Attack.Magic)}"));
            PushUpdate(new SetInterfaceTextPacket(1679, $"Range: {Format(Stats.Attack.Ranged)}"));

            PushUpdate(new SetInterfaceTextPacket(1680, $"Stab: {Format(Stats.Defense.Stab)}"));
            PushUpdate(new SetInterfaceTextPacket(1681, $"Slash: {Format(Stats.Defense.Slash)}"));
            PushUpdate(new SetInterfaceTextPacket(1682, $"Crush: {Format(Stats.Defense.Crush)}"));
            PushUpdate(new SetInterfaceTextPacket(1683, $"Magic: {Format(Stats.Defense.Magic)}"));
            PushUpdate(new SetInterfaceTextPacket(1684, $"Range: {Format(Stats.Defense.Ranged)}"));

            PushUpdate(new SetInterfaceTextPacket(1685, $"Strength: {Format(Stats.StrengthBonus)}     Range: {Format(Stats.RangedBonus)}"));
            PushUpdate(new SetInterfaceTextPacket(1686, $"Magic: {Format(Stats.MagicBonus)}"));
            PushUpdate(new SetInterfaceTextPacket(1687, $"Prayer: {Format(Stats.PrayerBonus)}"));
        }

        /*
        public override int Count(int id)
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
        */
    }
}