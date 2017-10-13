using System;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class ItemActionMetadata
    {
        public ItemActionType Type { get; }
        [NotNull]
        public IItemContainer Container { get; }

        public InterfaceMetadata Interface { get; }

        public int Index { get; }

        public ItemActionMetadata(
            ItemActionType type, 
            [NotNull] IItemContainer container,
            InterfaceMetadata @interface, 
            int index)
        {
            Type = type;
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Interface = @interface;
            Index = index;
        }
    }
}