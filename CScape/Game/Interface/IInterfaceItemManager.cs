namespace CScape.Game.Interface
{
    /// <summary>
    /// Safely manages item adding, removing and look up on the items of an underlying item provider.
    /// </summary>
    public interface IInterfaceItemManager
    {
        int Count { get; }
        IItemProvider Provider { get; }
        int Size { get; }

        ItemProviderChangeInfo CalcChangeInfo(int id, int amount);
        int Contains(int id);
        void ExecuteChangeInfo(ItemProviderChangeInfo info);
    }
}