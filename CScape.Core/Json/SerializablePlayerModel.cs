using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Item;
using CScape.Core.Game.Skill;
using CScape.Models.Game.Item;
using CScape.Models.Game.Skill;
using JetBrains.Annotations;

namespace CScape.Core.Database
{
    public class SerializablePlayerModel
    {
        [NotNull]
        public static SerializablePlayerModel Default(string username, SkillDb db)
         => new SerializablePlayerModel(
             3220, 3218, 0,
             new Dictionary<SkillID, ISkillModel>
             {
                 {db.Attack, new NormalSkillModel(db.Attack, 0, 0)},
                 {db.Defense, new NormalSkillModel(db.Defense, 0, 0)},
                 {db.Strength, new NormalSkillModel(db.Strength, 0, 0)},
                 {db.Hitpoints, new NormalSkillModel(db.Hitpoints, 0, 0)},
                 {db.Ranged, new NormalSkillModel(db.Ranged, 0, 0)},
                 {db.Prayer, new NormalSkillModel(db.Prayer, 0, 0)},
                 {db.Magic, new NormalSkillModel(db.Magic, 0, 0)},
                 {db.Cooking, new NormalSkillModel(db.Cooking, 0, 0)},
                 {db.Woodcutting, new NormalSkillModel(db.Woodcutting, 0, 0)},
                 {db.Fletching, new NormalSkillModel(db.Fletching, 0, 0)},
                 {db.Fishing, new NormalSkillModel(db.Fishing, 0, 0)},
                 {db.Firemaking, new NormalSkillModel(db.Firemaking, 0, 0)},
                 {db.Crafting, new NormalSkillModel(db.Crafting, 0, 0)},
                 {db.Smithing, new NormalSkillModel(db.Smithing, 0, 0)},
                 {db.Mining, new NormalSkillModel(db.Mining, 0, 0)},
                 {db.Herblore, new NormalSkillModel(db.Herblore, 0, 0)},
                 {db.Agility, new NormalSkillModel(db.Agility, 0, 0)},
                 {db.Thieving, new NormalSkillModel(db.Thieving, 0, 0)},
                 {db.Slayer, new NormalSkillModel(db.Slayer, 0, 0)},
                 {db.Farming, new NormalSkillModel(db.Farming, 0, 0)},
                 {db.Runecrafting, new NormalSkillModel(db.Runecrafting, 0, 0)}
             },
             Enumerable.Repeat(ItemStack.Empty, PlayerInventoryComponent.BackpackSize).ToArray(),
             Enumerable.Repeat(ItemStack.Empty, PlayerInventoryComponent.BankSize).ToArray(),
             Enumerable.Repeat(ItemStack.Empty, PlayerEquipmentContainer.EquipmentMaxSize).ToArray(),
             username,
             (int)PlayerComponent.Title.Normal,
             PlayerAppearance.Default,
             10);


        public SerializablePlayerModel(int posX, int posY, int posZ,
            Dictionary<SkillID, ISkillModel> skils, IList<ItemStack> backpack, IList<ItemStack> bank, 
            IList<ItemStack> equipment, string username, int titleId, PlayerAppearance apperance, int health)
        {
            PosX = posX;
            PosY = posY;
            PosZ = posZ;
            Skils = skils;
            Backpack = backpack;
            Bank = bank;
            Equipment = equipment;
            Username = username;
            TitleId = titleId;
            Apperance = apperance;
            Health = health;
        }
        
        public int Health { get; }

        public string Username { get; }
        public int TitleId { get; }
        public PlayerAppearance Apperance { get; }

        public int PosX { get; }
        public int PosY { get; }
        public int PosZ { get; }

        public Dictionary<SkillID, ISkillModel> Skils { get; }

        public IList<ItemStack> Backpack { get; }
        public IList<ItemStack> Bank { get; }
        public IList<ItemStack> Equipment { get; }
    }
}