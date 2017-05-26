using System;
using CScape.Core.Game.Interface.Showable;
using CScape.Core.Injection;

namespace CScape.Core.Game.Entity
{
    public sealed class PlayerSkills
    {
        public Player Player { get; }

        private readonly IPlayerModel _model;
        private readonly SkillSyncMachine _sync;

        public class SkillModel
        {
            private readonly PlayerSkills _parent;
            private readonly int _levelUpInterfaceId;

            public byte Index { get; }
            public string Name { get; }

            public int Boost
            {
                get => _parent._model.Skills.Boost[Index];
                set
                {
                    _parent._model.Skills.Boost[Index] = value;
                    _parent._sync.UpdateSkill(this);
                }
            }

            public int Experience
            {
                get => _parent._model.Skills.Experience[Index];
                set
                {
                    _parent._model.Skills.Experience[Index] = value;
                    _parent._sync.UpdateSkill(this);
                } 
            }

            /// <summary>
            /// The player's level given by their experience.
            /// </summary>
            public int ExperienceLevel
            {
                get
                {
                    if (Experience >= 13034431) return 99;
                    if (Experience >= 11805606) return 98;
                    if (Experience >= 10692629) return 97;
                    if (Experience >= 9684577) return 96;
                    if (Experience >= 8771558) return 95;
                    if (Experience >= 7944614) return 94;
                    if (Experience >= 7195629) return 93;
                    if (Experience >= 6517253) return 92;
                    if (Experience >= 5902831) return 91;
                    if (Experience >= 5346332) return 90;
                    if (Experience >= 4842295) return 89;
                    if (Experience >= 4385776) return 88;
                    if (Experience >= 3972294) return 87;
                    if (Experience >= 3597792) return 86;
                    if (Experience >= 3258594) return 85;
                    if (Experience >= 2951373) return 84;
                    if (Experience >= 2673114) return 83;
                    if (Experience >= 2421087) return 82;
                    if (Experience >= 2192818) return 81;
                    if (Experience >= 1986068) return 80;
                    if (Experience >= 1798808) return 79;
                    if (Experience >= 1629200) return 78;
                    if (Experience >= 1475581) return 77;
                    if (Experience >= 1336443) return 76;
                    if (Experience >= 1210421) return 75;
                    if (Experience >= 1096278) return 74;
                    if (Experience >= 992895) return 73;
                    if (Experience >= 899257) return 72;
                    if (Experience >= 814445) return 71;
                    if (Experience >= 737627) return 70;
                    if (Experience >= 668051) return 69;
                    if (Experience >= 605032) return 68;
                    if (Experience >= 547953) return 67;
                    if (Experience >= 496254) return 66;
                    if (Experience >= 449428) return 65;
                    if (Experience >= 407015) return 64;
                    if (Experience >= 368599) return 63;
                    if (Experience >= 333804) return 62;
                    if (Experience >= 302288) return 61;
                    if (Experience >= 273742) return 60;
                    if (Experience >= 247886) return 59;
                    if (Experience >= 224466) return 58;
                    if (Experience >= 203254) return 57;
                    if (Experience >= 184040) return 56;
                    if (Experience >= 166636) return 55;
                    if (Experience >= 150872) return 54;
                    if (Experience >= 136594) return 53;
                    if (Experience >= 123660) return 52;
                    if (Experience >= 111945) return 51;
                    if (Experience >= 101333) return 50;
                    if (Experience >= 91721) return 49;
                    if (Experience >= 83014) return 48;
                    if (Experience >= 75127) return 47;
                    if (Experience >= 67983) return 46;
                    if (Experience >= 61512) return 45;
                    if (Experience >= 55649) return 44;
                    if (Experience >= 50339) return 43;
                    if (Experience >= 45529) return 42;
                    if (Experience >= 41171) return 41;
                    if (Experience >= 37224) return 40;
                    if (Experience >= 33648) return 39;
                    if (Experience >= 30408) return 38;
                    if (Experience >= 27473) return 37;
                    if (Experience >= 24815) return 36;
                    if (Experience >= 22406) return 35;
                    if (Experience >= 20224) return 34;
                    if (Experience >= 18247) return 33;
                    if (Experience >= 16456) return 32;
                    if (Experience >= 14833) return 31;
                    if (Experience >= 13363) return 30;
                    if (Experience >= 12031) return 29;
                    if (Experience >= 10824) return 28;
                    if (Experience >= 9730) return 27;
                    if (Experience >= 8740) return 26;
                    if (Experience >= 7842) return 25;
                    if (Experience >= 7028) return 24;
                    if (Experience >= 6291) return 23;
                    if (Experience >= 5624) return 22;
                    if (Experience >= 5018) return 21;
                    if (Experience >= 4470) return 20;
                    if (Experience >= 3973) return 19;
                    if (Experience >= 3523) return 18;
                    if (Experience >= 3115) return 17;
                    if (Experience >= 2746) return 16;
                    if (Experience >= 2411) return 15;
                    if (Experience >= 2107) return 14;
                    if (Experience >= 1833) return 13;
                    if (Experience >= 1584) return 12;
                    if (Experience >= 1358) return 11;
                    if (Experience >= 1154) return 10;
                    if (Experience >= 969) return 9;
                    if (Experience >= 801) return 8;
                    if (Experience >= 650) return 7;
                    if (Experience >= 512) return 6;
                    if (Experience >= 388) return 5;
                    if (Experience >= 276) return 4;
                    if (Experience >= 174) return 3;
                    if (Experience >= 83) return 2;
                    if (Experience >= 0) return 1;
                    return 0;
                }
            }

            /// <summary>
            /// The player's level given by their experience and boosts.
            /// </summary>
            public int Level => ExperienceLevel + Boost;

            public SkillModel(PlayerSkills parent, byte idx, int levelUpInterfaceId, string name)
            {
                _parent = parent;
                _levelUpInterfaceId = levelUpInterfaceId;
                Index = idx;
                Name = name;

                _parent._sync.UpdateSkill(this);
            }

            public void GainExperience(int exp)
            {
                var oldLevel = ExperienceLevel;
                Experience += exp;

                if (ExperienceLevel != oldLevel)
                {
                    // player leveled up
                    _parent.Player.Interfaces.TryShow(
                        new LevelUpDialogInterface(_levelUpInterfaceId, Name, ExperienceLevel));

                    _parent.Player.Effect = ParticleEffect.LevelUp;
                }
            }
        }

        public ISkillProvider Provider { get; }

        public SkillModel Defense { get; }
        public SkillModel Attack { get; }
        public SkillModel Strength{ get; }
        public SkillModel Hitpoints { get; }
        public SkillModel Ranged { get; }
        public SkillModel Prayer { get; }
        public SkillModel Magic { get; }
        public SkillModel Cooking { get; }
        public SkillModel Woodcutting { get; }
        public SkillModel Fletching { get; }
        public SkillModel Fishing { get; }
        public SkillModel Firemaking { get; }
        public SkillModel Crafting { get; }
        public SkillModel Smithing{ get; }
        public SkillModel Mining { get; }
        public SkillModel Herblore{ get; }
        public SkillModel Agility { get; }
        public SkillModel Thieving { get; }
        public SkillModel Slayer { get; }
        public SkillModel Farming { get; }
        public SkillModel Runecrafting { get; }

        public PlayerSkills(
            IServiceProvider services, Player player, IPlayerModel model, SkillSyncMachine sync)
        {
            Player = player;
            _model = model;
            _sync = sync;
            var db = services.ThrowOrGet<IInterfaceIdDatabase>();

            var p = model.Skills;
            Provider = p;

            Attack = new SkillModel(this, 0, db.AttackLevelUpDialog, "Attack");
            Defense = new SkillModel(this, 1, db.DefenceLevelUpDialog, "Defense");
            Strength = new SkillModel(this, 2,db.StrengthLevelUpDialog, "Strength");
            Hitpoints = new SkillModel(this, 3, db.HitpointsLevelUpDialog, "Hitpoints");
            Ranged = new SkillModel(this, 4, db.RangedLevelUpDialog, "Ranged ");
            Prayer = new SkillModel(this, 5, db.PrayerLevelUpDialog, "Prayer");
            Magic = new SkillModel(this, 6, db.MagicLevelUpDialog, "Magic");
            Cooking = new SkillModel(this, 7, db.CookingLevelUpDialog, "Cooking");
            Woodcutting = new SkillModel(this, 8,db.WoodcuttingLevelUpDialog, "Woodcutting");
            Fletching = new SkillModel(this, 9,db.FletchingLevelUpDialog, "Fletching");
            Fishing = new SkillModel(this, 10, db.FishingLevelUpDialog, "Fishing");
            Firemaking = new SkillModel(this, 11, db.FiremakingLevelUpDialog, "Firemaking");
            Crafting = new SkillModel(this, 12, db.CraftingLevelUpDialog, "Crafting") ;
            Smithing = new SkillModel(this, 13, db.SmithingLevelUpDialog, "Smithing");
            Mining = new SkillModel(this, 14, db.MiningLevelUpDialog, "Mining");
            Herblore = new SkillModel(this, 15, db.HerbloreLevelUpDialog, "Herblore");
            Agility = new SkillModel(this, 16, db.AgilityLevelUpDialog, "Agility");
            Thieving = new SkillModel(this, 17, db.ThievingLevelUpDialog, "Thieving");
            Slayer = new SkillModel(this, 18, db.SlayerLevelUpDialog, "Slayer");
            Farming = new SkillModel(this, 19, db.FarmingLevelUpDialog, "Farming");
            Runecrafting = new SkillModel(this, 20, db.RunecraftingLevelUpDialog, "Runecrafting");
        }
    }
}