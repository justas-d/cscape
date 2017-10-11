using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Interface
{
    public struct InterfaceMetadata
    {
        public InterfaceType Type { get; }

        [NotNull]
        public IGameInterface Interface { get; }

        public int Index { get;}

        private InterfaceMetadata(InterfaceType type, IGameInterface interf, int index)
        {
            Type = type;
            Interface = interf;
            Index = index;
        }

        public static InterfaceMetadata Main(IGameInterface i) 
            => new InterfaceMetadata(InterfaceType.Main, i, -1);

        public static InterfaceMetadata Sidebar(IGameInterface i, int index)
            => new InterfaceMetadata(InterfaceType.Sidebar, i, index);

        public static InterfaceMetadata Chat(IGameInterface i)
            => new InterfaceMetadata(InterfaceType.Chat, i, -1);

        public static InterfaceMetadata Input(IGameInterface i)
            => new InterfaceMetadata(InterfaceType.Input, i, -1);
    }
}