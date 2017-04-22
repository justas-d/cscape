using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    /// <summary>
    /// Manages what an observer can see.
    /// </summary>
    public class Observatory : IEnumerable<UpdateObservable>
    {
        public IObserver Observer { get; }
        private readonly AbstractEntity _ent;

        private readonly List<UpdateObservable> _obs = new List<UpdateObservable>();
        private readonly HashSet<uint> _obsExisting = new HashSet<uint>();

        private readonly ObservableSyncMachine _obsSyncMachine;

        public Observatory(
            [NotNull] IObserver observer,
            [NotNull] ObservableSyncMachine obsSyncMachine)
        {
            Observer = observer ?? throw new ArgumentNullException(nameof(observer));

            var ent = Observer as AbstractEntity;
            if (ent != null)
                _ent = ent;

            _obsSyncMachine = obsSyncMachine ?? throw new ArgumentNullException(nameof(obsSyncMachine));
        }

        public void Clear()
        {
            _obs.Clear();
            _obsSyncMachine.Clear();
            _obsExisting.Clear();
        }

        private void InternalPushObservable(AbstractEntity obs)
        {
            var id = obs.UniqueEntityId;

            if (_obsExisting.Contains(id))
                return;

            if (!Observer.CanSee(obs))
                return;

            _obsExisting.Add(id);
            _obs.Add(new UpdateObservable(obs));
        }

        public void PushObservable(AbstractEntity obs)
        {
            var observer = obs as IObserver;

            if (observer != null && _ent != null  // check if we can both see each other.
                && !observer.Equals(_ent)) // make sure we don't recurse if pushing ourselves
            {
                observer.Observatory.InternalPushObservable(_ent);
            }

            InternalPushObservable(obs);
        }

        public IEnumerator<UpdateObservable> GetEnumerator()
        {
            // backwards iterate _obs so we can delete and yield in the same loop.
            for (var i = _obs.Count - 1; i >= 0; i--)
            {
                if (Observer.CanSee(_obs[i].Observable))
                    yield return _obs[i];
                else
                {
                    _obsExisting.Remove(_obs[i].Observable.UniqueEntityId);
                    _obs.RemoveAt(i);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}