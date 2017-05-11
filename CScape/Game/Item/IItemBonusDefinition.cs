namespace CScape.Game.Item
{
    public interface IItemBonusDefinition
    {
        int Slash { get; }
        int Crush { get; }
        int Stab { get; }
        int Magic { get; }
        int Ranged { get; }
    }
}