using System;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Item
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
        /// The name of the item
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The maximum stack amount of the item. Domain: (0, int.MaxValue]
        /// </summary>
        int MaxAmount { get; }
        // todo : ensure max amount domain: (0, int.MaxValue]

        /// <summary>
        /// Whether the item is tradable or not.
        /// </summary>
        bool IsTradable { get; }
        
        /// <summary>
        /// How much, in kg, one of the item weighs.
        /// </summary>
        float Weight { get; }

        /// <summary>
        /// Whether the item is noted or not.
        /// </summary>
        bool IsNoted { get; }

        /// <summary>
        /// If item is noted: the id of the real item ; If item isn't noted: if item is notable, the id of the note, else, -1.
        /// </summary>
        int NoteSwitchId { get; }

        void UseWith([NotNull] Player user, [NotNull] IItemDefinition other);
    }
}