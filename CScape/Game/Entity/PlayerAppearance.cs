namespace CScape.Game.Entity
{
    public class PlayerAppearance : IPlayerAppearance
    {
        public enum OverheadType : byte
        {
            None = 0
            // todo : overheads
        }

        public enum GenderType : byte
        {
            Male,
            Female
        }

        public int Head { get; }
        public int Chest { get; }
        public int Arms { get; }
        public int Hands { get; }
        public int Legs { get; }
        public int Feet { get; }
        public int Beard { get; }
        public GenderType Gender { get; }
        public OverheadType Overhead { get; }
        public byte SkinColor { get; }
        public byte HairColor { get; }
        public byte TorsoColor { get; }
        public byte LegColor { get; }
        public byte FeetColor { get; }

        public const int ChestIndex = 4;
        public const int ArmIndex = 6;
        public const int LegIndex = 7;
        public const int HeadIndex = 8;
        public const int HandIndex = 9;
        public const int FeetIndex = 10;
        public const int BeardIndex = 11;

        public int? this[int index]
        {
            get
            {
                switch (index)
                {
                    case ChestIndex:
                        return Chest;
                    case ArmIndex:
                        return Arms;
                    case LegIndex:
                        return Legs;
                    case HeadIndex:
                        return Head;
                    case HandIndex:
                        return Hands;
                    case FeetIndex:
                        return Feet;
                    case BeardIndex:
                        if (Gender == GenderType.Male)
                            return Beard;
                        return null;
                    default:
                        return null;
                }
            }
        }

        public static PlayerAppearance Default { get; } = new PlayerAppearance();

        /// <summary>
        /// Default constructor
        /// </summary>
        public PlayerAppearance() : this(0, 18, 26, 33, 36, 42, 10, 
            GenderType.Male, OverheadType.None, 0, 0, 0, 0, 0)
        {
            
        }

        public PlayerAppearance(int head, int chest, int arms, int hands, int legs, 
            int feet, int beard, GenderType gender, OverheadType overhead,
            byte skinColor, byte hairColor, byte torsoColor, byte legColor, byte feetColor)
        {

            Head = head;
            Chest = chest;
            Arms = arms;
            Hands = hands;
            Legs = legs;
            Feet = feet;
            Beard = beard;
            Gender = gender;
            Overhead = overhead;
            SkinColor = skinColor;
            HairColor = hairColor;
            TorsoColor = torsoColor;
            LegColor = legColor;
            FeetColor = feetColor;
        }

        public PlayerAppearance(IPlayerAppearance cpy)
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