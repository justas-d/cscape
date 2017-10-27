using System;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class ItemOnItemMessage : IGameMessage
    {
        public IItemContainer ContainerA { get; }
        public InterfaceMetadata MetaA { get; }
        public int IndexA { get; }
        public InterfaceMetadata MetaB { get; }
        public IItemContainer ContainerB { get; }
        public int IndexB { get; }
        public int EventId => (int) MessageId.ItemOnItemAction;

        public ItemOnItemMessage(
            InterfaceMetadata metaA, [NotNull] IItemContainer containerA, int indexA,
            InterfaceMetadata metaB, [NotNull] IItemContainer containerB, int indexB)
        {
            if (0 > indexA || containerA.Provider.Count >= indexA) throw new ArgumentOutOfRangeException(nameof(indexA));
            if (0 > indexB || containerB.Provider.Count >= indexB) throw new ArgumentOutOfRangeException(nameof(indexB));

            ContainerA = containerA ?? throw new ArgumentNullException(nameof(containerA));
            ContainerB = containerB ?? throw new ArgumentNullException(nameof(containerB));

            MetaA = metaA;
            IndexA = indexA;
            MetaB = metaB;
            IndexB = indexB;
        }

        public ItemStack GetItemA() => ContainerA.Provider[IndexA];
        public ItemStack GetItemB() => ContainerB.Provider[IndexB];
    }
}