namespace CScape.Core.Game.Entity
{
    // TODO : make PlayerAppearance a class
    public struct PlayerAppearance
    {
        public int Arms { get; }
        public int Beard { get; }
        public int Chest { get; }
        public int Feet { get; } 
        public byte FeetColor { get; } 
        public GenderType Gender { get; }
        public byte HairColor { get; } 
        public int Hands { get; } 
        public int Head { get; }
        public byte LegColor { get; }
        public int Legs { get; } 
        public byte SkinColor { get; }
        public byte TorsoColor { get; }

        public static PlayerAppearance Default { get; } = new PlayerAppearance(
            26,
            10,
            18,
            42,
            0,
            GenderType.Male,
            0,
            33,
            0,
            0,
            36,
            0,
            0);
    

        public PlayerAppearance(int arms, int beard, int chest, int feet, byte feetColor, GenderType gender, byte hairColor, int hands, int head, byte legColor, int legs, byte skinColor, byte torsoColor)
        {
            Arms = arms;
            Beard = beard;
            Chest = chest;
            Feet = feet;
            FeetColor = feetColor;
            Gender = gender;
            HairColor = hairColor;
            Hands = hands;
            Head = head;
            LegColor = legColor;
            Legs = legs;
            SkinColor = skinColor;
            TorsoColor = torsoColor;
        }
    }
}