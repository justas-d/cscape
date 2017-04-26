using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public sealed class PlayerModel : IPlayerAppearance
    {
        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public byte TitleIcon { get; set; } = 0;

        [NotNull] public string Username { get; set; }
        [NotNull] public string PasswordHash { get; set; }

        public ushort X { get; set; } = 3220;
        public ushort Y { get; set; } = 3218;
        public byte Z { get; set; } = 0;

        public int Head { get; set; } = 0;
        public int Chest { get; set; } = 18;
        public int Arms { get; set; } = 26;
        public int Hands { get; set; } = 33;
        public int Legs { get; set; } = 36;
        public int Feet { get; set; } = 42;
        public int Beard { get; set; } = 10;
        public PlayerAppearance.GenderType Gender { get; set; } = PlayerAppearance.GenderType.Male;
        public PlayerAppearance.OverheadType Overhead { get; set; } = PlayerAppearance.OverheadType.None;
        public byte SkinColor { get; set; } = 0;
        public byte HairColor { get; set; } = 0;
        public byte TorsoColor { get; set; } = 0;
        public byte LegColor { get; set; } = 0;
        public byte FeetColor { get; set; } = 0;

        private PlayerModel()
        {
            
        }

        public PlayerModel(string username, string password)
        {
            Username = username;
            PasswordHash = password;
        }

        public void SetPosition(Transform pos)
        {
            X = pos.X;
            Y = pos.Y;
            Z = pos.Z;
        }

        public void SetAppearance(PlayerAppearance cpy)
        {
            Head = cpy.Head;
            Chest = cpy.Chest;
            Arms = cpy.Arms;
            Hands = cpy.Hands;
            Legs = cpy.Legs;
            Feet = cpy.Feet;
            Beard = cpy.Beard;
            Gender = cpy.Gender;
            Overhead = cpy.Overhead;
            SkinColor = cpy.SkinColor;
            HairColor = cpy.HairColor;
            TorsoColor = cpy.TorsoColor;
            LegColor = cpy.LegColor;
            FeetColor = cpy.FeetColor;
        }
    }
}