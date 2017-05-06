using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.Model
{
    public sealed class PlayerModel
    {
        public const int BackpackSize = 28;

        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public byte TitleIcon { get; set; } = 0;

        [NotNull] public string Username { get; set; }
        [NotNull] public string PasswordHash { get; set; }

        public int X { get; set; } = 3220;
        public int Y { get; set; } = 3218;
        public byte Z { get; set; } = 0;

        public bool IsMember { get; set; } = true;

        public PlayerAppearance Appearance { get; set; } = PlayerAppearance.Default;
        public ModelItemProvider BackpackItems { get; set; }  = new ModelItemProvider(BackpackSize);

        private PlayerModel()
        {

        }

        public PlayerModel(string username, string password)
        {
            Username = username;
            PasswordHash = password;
        }

        public void SetPosition(ITransform pos)
        {
            X = pos.X;
            Y = pos.Y;
            Z = pos.Z;
        }
    }
}