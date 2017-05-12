using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Injection;

namespace CScape.Basic.Model
{
    public class PlayerModel : IPlayerModel
    {
        public const int BackpackSize = 28;

        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public byte TitleIcon { get; set; }

        public string Id { get; set; }
        public string PasswordHash { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public byte Z { get; set; }

        public bool IsMember { get; set; }

        public ItemProviderModel BackpackItems { get; private set; }
        public PlayerAppearance Appearance { get; private set; }

        IPlayerAppearance IPlayerModel.Appearance { get => Appearance; set => Appearance = (PlayerAppearance) value; }
        IItemProvider IPlayerModel.BackpackItems { get => BackpackItems; set => BackpackItems = (ItemProviderModel) value; }

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
            BackpackItems = new ItemProviderModel(BackpackSize);
        }

        public void UpdatePosition(ITransform pos)
        {
            X = pos.X;
            Y = pos.Y;
            Z = pos.Z;
        }
    }
}