using JetBrains.Annotations;

namespace CScape.Models.Game.Interface
{
    public struct InterfaceMetadata
    {
        /// <summary>
        /// The type of the interface
        /// </summary>
        public InterfaceType Type { get; }

        [NotNull]
        public IGameInterface Interface { get; }
        
        /// <summary>
        /// The sidebar index of the interface
        /// </summary>
        public int Index { get;}

        private InterfaceMetadata(InterfaceType type, IGameInterface interf, int index)
        {
            Type = type;
            Interface = interf;
            Index = index;
        }

        /// <summary>
        /// Creates metadata for interface which cannot be categorized under and of the interface types.
        /// This should simply add the interface to the interface component and allow it to be updated.
        /// </summary>
        public static InterfaceMetadata Register(IGameInterface i)
            => new InterfaceMetadata(InterfaceType.None, i, -1);

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