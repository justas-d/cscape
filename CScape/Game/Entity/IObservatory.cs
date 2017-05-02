using System.Collections.Generic;

namespace CScape.Game.Entity
{
    public interface IObservatory
    {
        /// <summary>
        /// The IObserver which owns this IObservatory
        /// </summary>
        IObserver Observer { get; }

        /// <summary>
        /// Removes all tracked IWorldEnties from this IObservatory.
        /// </summary>
        void Clear();

        /// <summary>
        /// Enumerates all tracked IWorldEntities
        /// </summary>
        /// <returns></returns>
        IEnumerator<UpdateObservable> GetEnumerator();

        /// <summary>
        /// Pushes a world entity to this observable.
        /// </summary>
        void PushObservable(IWorldEntity obs);

        /// <summary>
        /// Pushes a world entity to this observable AND to the world entity if it's an IObserver.
        /// </summary>
        void RecursivePushObservable(IWorldEntity obs);
    }
}