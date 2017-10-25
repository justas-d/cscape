using System.Collections.Generic;
using CScape.Core.Game.Entity;
using CScape.Models.Game.Item;
using CScape.Models.Game.Skill;

namespace CScape.Core.Database
{
    public class SerializablePlayerModel
    {
        public SerializablePlayerModel(int posX, int posY, int posZ, Dictionary<SkillID, ISkillModel> skils, IList<ItemStack> backpack, IList<ItemStack> bank, IList<ItemStack> equipment, string username, int titleId, PlayerAppearance apperance)
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
        }

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