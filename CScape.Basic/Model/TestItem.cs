using System;
using CScape.Core;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;
using CScape.Core.Injection;

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

        public void UseWith(Player player, IContainerInterface ourContainer, int ourIdx, IContainerInterface otherContainer,
            int otherIdx)
        {
            // :thinking:
            player.DebugMsg($"Use [i:{ItemId}x{ourContainer.Items.Provider.GetAmount(ourIdx)} with [i:{otherContainer.Items.Provider.GetId(otherIdx)}x{otherContainer.Items.Provider.GetAmount(otherIdx)}]", ref player.DebugItems);
        }

        public void OnAction(Player player, IContainerInterface container, int index, ItemActionType type)
        {
            var manager = container.Items;

            player.DebugMsg($"Action {type} on [i:{ItemId}x{manager.Provider.GetAmount(index)}] ", ref player.DebugItems);

            switch (type)
            {
                case ItemActionType.Generic1:
                    player.DebugMsg($"Equipping {ItemId}", ref player.DebugItems);

                    if (ItemHelper.InterManagerSwapPreserveIndex(manager, index, player.Equipment, (int)Slot,
                        player.Server.Services.ThrowOrGet<IItemDefinitionDatabase>()))
                    {
                        player.DebugMsg($"Success", ref player.DebugItems);

                    }
                    else
                        player.DebugMsg($"Fail", ref player.DebugItems);
                    break;
                case ItemActionType.Generic2:
                    break;
                case ItemActionType.Generic3:
                    break;
                case ItemActionType.Drop:

                    var item = container.Items.Provider[index];

                    // remove item
                    if (!container.Items.ExecuteChangeInfo(ItemProviderChangeInfo.Remove(index)))
                        return;

                    // drop item
                    new GroundItem(player.Server.Services, item, player.Transform, player, player.Transform.PoE);

                    break;
                case ItemActionType.Remove:
                    player.DebugMsg($"Removing {ItemId}", ref player.DebugItems);
                    if(ItemHelper.RemoveFromA_AddToB(manager, index, player.Inventory))
                    {
                        player.DebugMsg($"Success", ref player.DebugItems);
                    }
                    else
                        player.DebugMsg($"Fail", ref player.DebugItems);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public bool Equals(IItemDefinition other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ItemId == other.ItemId;
        }

        public EquipSlotType Slot { get; } = EquipSlotType.Head;
        public IItemBonusDefinition Attack { get; } = null;
        public IItemBonusDefinition Defence { get; } = null;
        public int StrengthBonus { get; } = 1;
        public int MagicBonus { get; } = 2;
        public int RangedBonus { get; } = 3;
        public int PrayerBonus { get; } = 4;
        public AttackStyle[] Styles { get; } = null;

        public bool CanEquip(Player player) => true;

        public void OnEquip(Player player, IContainerInterface container, int idx)
        {
        }
    }
}