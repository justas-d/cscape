using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Combat;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class CombatStatComponent : EntityComponent, ICombatStatComponent
    {
        private sealed class Stats : IEquipmentStats
        {
            public int Slash { get; set; }
            public int Crush { get; set; }
            public int Stab { get; set; }
            public int Magic { get; set; }
            public int Ranged { get; set; }
        }

        private readonly Stats _attack = new Stats();
        private readonly Stats _defense = new Stats();

        [NotNull]
        public IEquipmentStats Attack => _attack;

        [NotNull]
        public IEquipmentStats Defense => _defense;

        public int StrengthBonus { get; private set; }
        public int MagicBonus { get; private set; }
        public int RangedBonus { get; private set; }
        public int PrayerBonus { get; private set; }

        public override int Priority => (int)ComponentPriority.CombatStatComponent;

        public CombatStatComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        /// <summary>
        /// Updates the values based off of the items int he given equipment manager.
        /// </summary>
        public void Update(IItemContainer equipment)
        {
            // reset
            StrengthBonus = 0;
            MagicBonus = 0;
            RangedBonus = 0;
            PrayerBonus = 0;

            void UpdateStats(Stats our, IEquipmentStats item)
            {
                // reset
                our.Slash = 0;
                our.Crush = 0;
                our.Stab = 0;
                our.Magic = 0;
                our.Ranged = 0;

                if (item == null) return;

                // update
                our.Slash += item.Slash;
                our.Crush += item.Crush;
                our.Stab += item.Stab;
                our.Magic += item.Magic;
                our.Ranged += item.Ranged;
            }

            // update
            foreach (var t in equipment.Provider)
            {
                var def = t.Id as IEquippableItem;

                if (def == null)
                    continue;

                StrengthBonus += def.StrengthBonus;
                MagicBonus += def.MagicBonus;
                RangedBonus += def.RangedBonus;
                PrayerBonus += def.PrayerBonus;

                UpdateStats(_attack, def.Attack);
                UpdateStats(_defense, def.Defence);
            }
        }
        
        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.EquipmentChange:
                    Update(msg.AsEquipmentChange().Container);
                    break;
            }
            
        }
    }
}