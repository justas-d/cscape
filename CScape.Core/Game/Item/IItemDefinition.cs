using System;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;

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
        
        /// <summary>
        /// Called whenever this type of item is used with another item.
        /// </summary>
        void UseWith(Player player, 
            IContainerInterface ourContainer, int ourIdx, 
            IContainerInterface otherContainer, int otherIdx);

        /// <summary>
        /// CAlled whenever an action occurs on an item with this definition.
        /// </summary>
        /// <param name="container">The container that this item belongs to.</param>
        /// <param name="index">The index in the container of this item.</param>
        /// <param name="type">The type of action used on this item.</param>
        void OnAction(Player player, IContainerInterface container, int index, ItemActionType type);
    }
}