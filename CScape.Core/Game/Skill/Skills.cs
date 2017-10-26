using System;
using CScape.Core.Database;
using CScape.Models.Game.Skill;

namespace CScape.Core.Game
{
    public class SkillDb
    {
        public SkillID Attack { get; }
        public SkillID Defense { get; }
        public SkillID Strength { get; }
        public SkillID Hitpoints { get; }
        public SkillID Ranged { get; }
        public SkillID Prayer { get; }
        public SkillID Magic { get; }
        public SkillID Cooking { get; }
        public SkillID Woodcutting { get; }
        public SkillID Fletching { get; }
        public SkillID Fishing { get; }
        public SkillID Firemaking { get; }
        public SkillID Crafting { get; }
        public SkillID Smithing { get; }
        public SkillID Mining { get; }
        public SkillID Herblore { get; }
        public SkillID Agility { get; }
        public SkillID Thieving { get; }
        public SkillID Slayer { get; }
        public SkillID Farming { get; }
        public SkillID Runecrafting { get; }

        public SkillDb(IServiceProvider services)
        {
            var db = services.ThrowOrGet<InterfaceIdDatabase>();
            Attack = new SkillID("Attack", 0, db.AttackLevelUpDialog);
            Defense = new SkillID("Defense", 1, db.DefenceLevelUpDialog);
            Strength = new SkillID("Strength", 2, db.StrengthLevelUpDialog);
            Hitpoints = new SkillID("Hitpoints", 3, db.HitpointsLevelUpDialog);
            Ranged = new SkillID("Ranged", 4, db.RangedLevelUpDialog);
            Prayer = new SkillID("Prayer", 5, db.PrayerLevelUpDialog);
            Magic = new SkillID("Magic", 6, db.MagicLevelUpDialog);
            Cooking = new SkillID("Cooking", 7, db.CookingLevelUpDialog);
            Woodcutting = new SkillID("Woodcutting", 8, db.WoodcuttingLevelUpDialog);
            Fletching = new SkillID("Fletching", 9, db.FletchingLevelUpDialog);
            Fishing = new SkillID("Fishing", 10, db.FishingLevelUpDialog);
            Firemaking = new SkillID("Firemaking", 11, db.FiremakingLevelUpDialog);
            Crafting = new SkillID("Crafting", 12, db.CraftingLevelUpDialog);
            Smithing = new SkillID("Smithing", 13, db.SmithingLevelUpDialog);
            Mining = new SkillID("Mining", 14, db.MiningLevelUpDialog);
            Herblore = new SkillID("Herblore", 15, db.HerbloreLevelUpDialog);
            Agility = new SkillID("Agility", 16, db.AgilityLevelUpDialog);
            Thieving = new SkillID("Thieving", 17, db.ThievingLevelUpDialog);
            Slayer = new SkillID("Slayer", 18, db.SlayerLevelUpDialog);
            Farming = new SkillID("Farming", 19, db.FarmingLevelUpDialog);
            Runecrafting = new SkillID("Runecrafting", 20, db.RunecraftingLevelUpDialog);
        }
    }
}