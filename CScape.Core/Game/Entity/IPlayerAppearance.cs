using CScape.Core.Game.Entities;

namespace CScape.Core.Game.Entity
{
    public interface IPlayerAppearance
    {
        int Arms { get; set; }
        int Beard { get; set; }
        int Chest { get; set; }
        int Feet { get; set; }
        byte FeetColor { get; set; }
        GenderType Gender { get; set; }
        byte HairColor { get; set; }
        int Hands { get; set; }
        int Head { get; set; }
        byte LegColor { get; set; }
        int Legs { get; set; }
        OverheadType Overhead { get; set; }
        byte SkinColor { get; set; }
        byte TorsoColor { get; set; }

        int? this[int index] { get; }
    }
}