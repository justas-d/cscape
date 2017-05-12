using CScape.Core.Game.Entity;

namespace CScape.Basic.Model
{
    /// <summary>
    /// Mutable appearance model. Changing values here will not sync.
    /// </summary>
    public class PlayerAppearance : PlayerModelLeaf, IPlayerAppearance
    {
        public int Head { get; set; } = 0;
        public int Chest { get; set; } = 18;
        public int Arms { get; set; } = 26;
        public int Hands { get; set; } = 33;
        public int Legs { get; set; } = 36;
        public int Feet { get; set; } = 42;
        public int Beard { get; set; } = 10;
        public GenderType Gender { get; set; } = GenderType.Male;
        public OverheadType Overhead { get; set; } = OverheadType.None;
        public byte SkinColor { get; set; } = 0;
        public byte HairColor { get; set; } = 0;
        public byte TorsoColor { get; set; } = 0;
        public byte LegColor { get; set; } = 0;
        public byte FeetColor { get; set; } = 0;

        #region indexer
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
#endregion

        public PlayerAppearance() { }
    }
}