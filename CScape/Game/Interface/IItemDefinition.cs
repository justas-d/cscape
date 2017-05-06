using System;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines how an item with a unique id should behave.
    /// </summary>
    public interface IItemDefinition : IEquatable<IItemDefinition>
    {
        /// <summary>
        /// The unique id of the item.
        /// </summary>
        int ItemId { get; }

        /// <summary>
        /// The maximum stack amount of the item. Domain: (0, int.MaxValue]
        /// </summary>
        int MaxAmount { get; }
        // todo : ensure max amount domain: (0, int.MaxValue]
    }
}