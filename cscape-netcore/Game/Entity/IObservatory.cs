using System.Collections.Generic;

namespace CScape.Game.Entity
{
    public interface IObservatory
    {
        IObserver Observer { get; }

        void Clear();
        IEnumerator<UpdateObservable> GetEnumerator();
        void PushObservable(AbstractEntity obs);
    }
}