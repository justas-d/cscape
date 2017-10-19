namespace CScape.Models.Game.Entity.Component
{
    public interface IInterfaceComponent
    {
        System.Collections.Generic.IReadOnlyDictionary<int, InterfaceMetadata> All { get; }
        IGameInterface Chat { get; }
        IGameInterface Input { get; }
        IGameInterface Main { get; }
        System.Collections.Generic.IList<IGameInterface> Sidebar { get; }

        bool CanShow(InterfaceMetadata meta);
        void Close(int id);
        void Show(InterfaceMetadata meta);
    }
}