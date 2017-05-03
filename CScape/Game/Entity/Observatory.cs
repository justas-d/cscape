using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    public class PlayerObservatory : IObservatory
    {
        public IObserver Observer { get; }

        private readonly HashSet<IWorldEntity> _seeableEntities = new HashSet<IWorldEntity>();
        private readonly HashSet<uint> _newEntityIds = new HashSet<uint>();

        public ObservableSyncMachine Sync { get; }

        public bool ReevaluateSightOverride { get; set; }

        public PlayerObservatory([NotNull] Player observer)
        {
            Observer = observer ?? throw new ArgumentNullException(nameof(observer));
            Sync = new ObservableSyncMachine(observer, this);
            observer.Connection.SyncMachines.Add(Sync);
        }

        public void Clear()
        {
            // perform a double ended clear
            foreach (var ent in _seeableEntities.OfType<IObserver>().Where(e => !e.Equals(Observer)))
                ent.Observatory.Remove(Observer);

            _seeableEntities.Clear();
            _newEntityIds.Clear();
            Sync.Clear();
        }

        public void Remove(IWorldEntity ent)
        {
            _seeableEntities.Remove(ent);
            _newEntityIds.Remove(ent.UniqueEntityId);
        }

        /// <summary>
        /// Enumerates all tracked IWorldEntities
        /// </summary>
        public IEnumerator<IWorldEntity> GetEnumerator()
        {
            // re evaluate the sightlines for all entities in our region that require it.
            foreach (var ent in Observer.Position.Region.GetNearbyInclusive().SelectMany(e => e.WorldEntities))
            {
                if (ReevaluateSightOverride || ent.NeedsSightEvaluation)
                    DoubleEndedPushObservable(ent);
            }

            ReevaluateSightOverride = false;

            // return the entities that we can see.
            return _seeableEntities.GetEnumerator();
        }

        public void PushObservable(IWorldEntity ent)
        {
            if (ent == null)
                return;

            // can see : keep or add
            if (Observer.CanSee(ent))
            {
                // keep
                if (_seeableEntities.Contains(ent))
                    return;

                // add
                _seeableEntities.Add(ent);
                _newEntityIds.Add(ent.UniqueEntityId);
            }
            // can't see : remove
            else
               Remove(ent);
        }

        public void DoubleEndedPushObservable(IWorldEntity ent)
        {
            if (ent == null)
                return;

            // push our observable to the other entities observatory if it has one.
            var asObs = ent as IObserver;
            if (!ent.Equals(Observer)) // make sure we don't do this if we're pushing our own observer
                asObs?.Observatory.PushObservable(Observer);

            // push new entity to observables
            PushObservable(ent);
        }

        public bool PopIsNew(IWorldEntity ent)
        {
            if (ent == null) return false;

            if (_newEntityIds.Contains(ent.UniqueEntityId))
            {
                _newEntityIds.Remove(ent.UniqueEntityId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enumerates all tracked IWorldEntities
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}