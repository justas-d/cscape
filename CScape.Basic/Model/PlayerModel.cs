using CScape.Core.Game.Entities.Skill;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Injection;

namespace CScape.Basic.Model 
{
    public class PlayerModel : IPlayerModel
    {
        public const int BackpackSize = 28;
        public const int EquipmentMaxSize = EquipmentManager.EquipmentMaxSize;

        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public byte TitleIcon { get; set; }

        IPlayerAppearance IPlayerModel.Appearance
        {
            get => Appearance;
            set => Appearance = (PlayerAppearance)value;
        }

        private ItemProviderSegment _backpack;
        private ItemProviderSegment _equipment;

        IItemProvider IPlayerModel.BackpackItems
        {
            get => _backpack;
            set => _backpack = (ItemProviderSegment) value;
        }

        IItemProvider IPlayerModel.Equipment
        {
            get => _equipment;
            set => _equipment = (ItemProviderSegment) value;
        }

        ISkillProvider IPlayerModel.Skills
        {
            get => Skills;
            set => Skills = (DbSkillModel) value;
        }

        public string Id { get; set; }
        public string PasswordHash { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public byte Z { get; set; }

        public byte Health { get; set; } = 10;

        public bool IsMember { get; set; }


        private ItemProviderModel _items;

        public ItemProviderModel Items
        {
            get => _items;
            set
            {
                // create segments
                _items = value;
                _backpack = new ItemProviderSegment(_items, 0, BackpackSize);
                _equipment = new ItemProviderSegment(_items, BackpackSize, BackpackSize + EquipmentMaxSize);
            }
        }
        public PlayerAppearance Appearance { get; set; }

        public DbSkillModel Skills { get; set; }

        public PlayerModel()
        {
        }

        public PlayerModel(string id, string password)
        {
            Id = id;
            TitleIcon = 0;
            X = 3220;
            Y = 3218;
            Z = 0;
            IsMember = true;
            PasswordHash = password;

            Appearance = new PlayerAppearance();
            Items = new ItemProviderModel(BackpackSize + EquipmentMaxSize);
            Skills = new DbSkillModel(21);
        }
    }
}