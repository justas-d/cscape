using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
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

        public override int Priority { get; }

        public CombatStatComponent([NotNull] Entity parent) : base(parent)
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
        
        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.EquipmentChange:
                    Update(msg.AsEquipmentChange().Container);
                    break;
            }
            
        }
    }
}