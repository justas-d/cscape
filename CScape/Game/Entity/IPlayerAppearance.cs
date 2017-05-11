namespace CScape.Game.Entity
{
    public interface IPlayerAppearance
    {
        int Arms { get; }
        int Beard { get; }
        int Chest { get; }
        int Feet { get; }
        byte FeetColor { get; }
        GenderType Gender { get; }
        byte HairColor { get; }
        int Hands { get; }
        int Head { get; }
        byte LegColor { get; }
        int Legs { get; }
        OverheadType Overhead { get; }
        byte SkinColor { get; }
        byte TorsoColor { get; }
    }
}