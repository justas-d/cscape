namespace CScape.Models.Game.Combat
{
    public interface IEquipmentStats
    {
        int Slash { get; }
        int Crush { get; }
        int Stab { get; }
        int Magic { get; }
        int Ranged { get; }
    }
}