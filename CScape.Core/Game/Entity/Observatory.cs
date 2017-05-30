using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CScape.Core.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public class PlayerObservatory : IObservatory
    {
        public IObserver Observer { get; }

        private ImmutableHashSet<IWorldEntity> _seeableEntities = ImmutableHashSet<IWorldEntity>.Empty;
        private readonly HashSet<uint> _newEntityIds = new HashSet<uint>();

        public ObservableSyncMachine Sync { get; }

        public bool ReevaluateSightOverride { get; set; }

        public PlayerObservatory(IServiceProvider services, [NotNull] Player observer)
        {
            Observer = observer ?? throw new ArgumentNullException(nameof(observer));
            Sync = new ObservableSyncMachine(services, observer, this);
            observer.Connection.SyncMachines.Add(Sync);
        }

        public void Clear()
        {
            // perform a double ended clear
            foreach (var ent in _seeableEntities.OfType<IObserver>().Where(e => !e.Equals(Observer)))
                ent.Observatory.Remove(Observer);

            _seeableEntities = ImmutableHashSet<IWorldEntity>.Empty;
            _newEntityIds.Clear();
            Sync.Clear();
            ReevaluateSightOverride = true;
        }

        public void Remove(IWorldEntity ent)
        {
            if (!_seeableEntities.Contains(ent)) return;

            _seeableEntities = _seeableEntities.Remove(ent);
            _newEntityIds.Remove(ent.UniqueEntityId);

            if (ent is Npc n)
                Sync.NpcSync.Remove(n);
            if(ent is Player p)
                Sync.PlayerSync.Remove(p);
        }

        /// <summary>
        /// Enumerates all tracked IWorldEntities
        /// </summary>
        public IEnumerator<IWorldEntity> GetEnumerator()
        {
            var evaluated = new HashSet<uint>();

            void EvalSight(IWorldEntity ent)
            {
                if((ReevaluateSightOverride || ent.NeedsSightEvaluation) && !evaluated.Contains(ent.UniqueEntityId))
                { 
                    DoubleEndedPushObservable(ent);
                    evaluated.Add(ent.UniqueEntityId);
                }
            }

            // re evaluate the sightlines for all entities in our region that require it.
            foreach (var ent in Observer.Transform.Region.GetNearbyInclusive()
                .SelectMany(e => e.WorldEntities)
                .Where(e => !e.Equals(Observer)))
            {
                EvalSight(ent);
            }
                

            ReevaluateSightOverride = false;

            // return the entities that we can see and that aren't destroyed.
            foreach (var ent in _seeableEntities)
            {
                EvalSight(ent);
                yield return ent;
            }
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
                _seeableEntities = _seeableEntities.Add(ent);
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