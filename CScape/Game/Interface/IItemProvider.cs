namespace CScape.Game.Interface
{
    /// <summary>
    /// Provides items to an interface item manager.
    /// </summary>
    public interface IItemProvider
    {
        (int id, int amount)[] Items { get; }
    }
}