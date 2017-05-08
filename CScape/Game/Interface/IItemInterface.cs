namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines an interface that has and manages an item container.
    /// </summary>
    public interface IItemInterface : IInterface
    {
        IItemManager Items { get; }
    }
}