using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Entity.Segment;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{

    [RequiresComponent(typeof(NetworkingComponent))]
    public abstract class EntityNetworkSyncComponent : EntityComponent
    {
        private List<IEntityHandle> SyncEntities { get; }
            = new List<IEntityHandle>();

        private List<IEntityHandle> InitEntities { get; }
            = new List<IEntityHandle>();

        private HashSet<IEntityHandle> LeaveEntities { get; }
            = new HashSet<IEntityHandle>();
        
        protected EntityNetworkSyncComponent(
            [NotNull] IEntity parent) : base(parent)
        {

        }

        private void AddEntity([NotNull] IEntityHandle ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            Debug.Assert(!SyncEntities.Contains(ent));
            Debug.Assert(!LeaveEntities.Contains(ent));

            InitEntities.Add(ent);
        }

        private void RemoveEntity([NotNull] IEntityHandle ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            Debug.Assert(SyncEntities.Contains(ent));
            Debug.Assert(!InitEntities.Contains(ent));

            LeaveEntities.Add(ent);
        }

        protected abstract bool IsHandleableEntity(IEntityHandle h);

        protected IUpdateSegment CommonSegmentResolve(
            FlagAccumulatorComponent flags, bool needsUpdate)
        {

            if (flags.Movement != null)
            {
                if (flags.Movement.IsWalking)
                {
                    return new EntityMovementWalkSegment(
                        flags.Movement.Dir1.Direction, needsUpdate);
                }
                else
                {
                    return new EntityMovementRunSegment(
                        flags.Movement.Dir1.Direction,
                        flags.Movement.Dir2.Direction,
                        needsUpdate);
                }
            }
            if (needsUpdate)
                return OnlyNeedsUpdateSegment.Instance;

            return NoUpdateSegment.Instance;
        }


        protected IEnumerable<IUpdateSegment> GetSyncSegments(
            IList<IUpdateWriter> updateSegments,
            Func<FlagAccumulatorComponent, IUpdateWriter> updateWriterFactory)
        {
            var removeList = new List<IEntityHandle>();
            var syncSegments = new List<IUpdateSegment>();

            foreach (var handle in SyncEntities)
            {
                // remove entity if
                // it's dead
                // or its in the leave list
                if (handle.IsDead() ||
                    LeaveEntities.Contains(handle))
                {
                    syncSegments.Add(RemoveEntitySegment.Instance);
                    removeList.Add(handle);
                }
                else
                {
                    var entity = handle.Get();
                    var flags = entity.Components.AssertGet<FlagAccumulatorComponent>();

                    if (flags.Reinitialize)
                    {
                        InitEntities.Add(handle);
                        removeList.Add(handle);
                    }
                    else
                    {
                        var updater = updateWriterFactory(flags);
                        var needsUpdate = updater.NeedsUpdate();

                        syncSegments.Add(CommonSegmentResolve(flags, needsUpdate));

                        if (needsUpdate)
                        {
                            updateSegments.Add(updater);
                        }
                    }
                }
            }
            
            // cleanup
            LeaveEntities.Clear();

            foreach (var handle in removeList)
            {
                SyncEntities.Remove(handle);
            }

            return syncSegments;
        }

        protected abstract void SetInitialFlags(IUpdateWriter writer, IEntity ent);

        protected IEnumerable<IUpdateSegment> GetInitSegments(
            IList<IUpdateWriter> updateSegments,
            Func<FlagAccumulatorComponent, IUpdateWriter> updateWriterFactory)
        {
            var init = new List<IUpdateSegment>();

            /* Initialize */
            foreach (var entity in InitEntities
                .Where(h => !h.IsDead())
                .Select(h => h.Get()))
            {
                var flags = entity.Components.AssertGet<FlagAccumulatorComponent>();
                var updater = updateWriterFactory(flags);

                SyncEntities.Add(entity.Handle);
                SetInitialFlags(updater, entity);

                var needsUpd = updater.NeedsUpdate();

                init.Add(new InitPlayerSegment(entity.AssertGetPlayer(), Parent.AssertGetPlayer(), needsUpd));

                if (needsUpd)
                {
                    updateSegments.Add(updater);
                }
            }

            InitEntities.Clear();
            return init;
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.NetworkReinitialize:
                {
                    // todo : maybe send remove entity segments when resetting?
                    SyncEntities.Clear();
                    InitEntities.Clear();
                    break;
                }
                case (int)MessageId.NetworkUpdate:
                {
                    Sync();
                    break;
                }
                case (int)MessageId.EntityEnteredViewRange:
                {
                    var h = msg.AsEntityEnteredViewRange();
                    if (IsHandleableEntity(h.Entity))
                        AddEntity(h.Entity);

                    break;
                }
                case (int)MessageId.EntityLeftViewRange:
                {
                    var h = msg.AsEntityLeftViewRange();
                    if (IsHandleableEntity(h.Entity))
                        RemoveEntity(h.Entity);

                    break;
                }
            }
        }

        protected abstract void Sync();
    }
}