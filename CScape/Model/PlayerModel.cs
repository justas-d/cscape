using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Model
{
    public class PlayerModel
    {
        public const int BackpackSize = 28;

        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public byte TitleIcon { get; set; }

        [NotNull] public string Id { get; set; }
        [NotNull] public string PasswordHash { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public byte Z { get; set; }

        public bool IsMember { get; set; }


        public ItemProviderModel BackpackItems { get; set; }
        public PlayerAppearance Appearance { get; set; }

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

        public void SetPosition(ITransform pos)
        {
            X = pos.X;
            Y = pos.Y;
            Z = pos.Z;
        }
    }
}