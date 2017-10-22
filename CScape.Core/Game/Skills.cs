using CScape.Models.Game.Skill;

namespace CScape.Core.Game
{
    public static class Skills
    {
        public static SkillID Attack { get; } = new SkillID("Attack", 0);
        public static SkillID Defense { get; } = new SkillID("Defense", 1);
        public static SkillID Strength { get; } = new SkillID("Strength", 2);
        public static SkillID Hitpoints { get; } = new SkillID("Hitpoints", 3);
        public static SkillID Ranged { get; } = new SkillID("Ranged", 4);
        public static SkillID Prayer { get; } = new SkillID("Prayer", 5);
        public static SkillID Magic { get; } = new SkillID("Magic", 6);
        public static SkillID Cooking { get; } = new SkillID("Cooking", 7);
        public static SkillID Woodcutting { get; } = new SkillID("Woodcutting", 8);
        public static SkillID Fletching { get; } = new SkillID("Fletching", 9);
        public static SkillID Fishing { get; } = new SkillID("Fishing", 10);
        public static SkillID Firemaking { get; } = new SkillID("Firemaking", 11);
        public static SkillID Crafting { get; } = new SkillID("Crafting", 12);
        public static SkillID Smithing { get; } = new SkillID("Smithing", 13);
        public static SkillID Mining { get; } = new SkillID("Mining", 14);
        public static SkillID Herblore { get; } = new SkillID("Herblore", 15);
        public static SkillID Agility { get; } = new SkillID("Agility", 16);
        public static SkillID Thieving { get; } = new SkillID("Thieving", 17);
        public static SkillID Slayer { get; } = new SkillID("Slayer", 18);
        public static SkillID Farming { get; } = new SkillID("Farming", 19);
        public static SkillID Runecrafting { get; } = new SkillID("Runecrafting", 20);
    }
}