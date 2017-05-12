using System.Collections.Generic;

namespace CScape.Core.Game.Entity
{
    public interface IObservatory : IEnumerable<IWorldEntity>
    {
        /// <summary>
        /// When set, will re-evaluate all sight for all region-local players.
        /// Is automatically unset after that has been done.
        /// </summary>
        bool ReevaluateSightOverride { get; set; }

        /// <summary>
        /// The IObserver which owns this IObservatory
        /// </summary>
        IObserver Observer { get; }

        /// <summary>
        /// Removes all tracked IWorldEnties from this IObservatory.
        /// </summary>
        void Clear();

        /// <summary>
        /// Pushes a world entity to this observable.
        /// </summary>
        void PushObservable(IWorldEntity ent);

        /// <summary>
        /// Pushes a world entity to this observable AND to the world entity if it's an IObserver.
        /// </summary>
        void DoubleEndedPushObservable(IWorldEntity obs);

        /// <summary>
        /// Pops and returns the is new value for the entity.
        /// </summary>
        bool PopIsNew(IWorldEntity ent);

        /// <summary>
        /// Removes the given entity from the observatory.
        /// </summary>
        void Remove(IWorldEntity ent);

    }
}