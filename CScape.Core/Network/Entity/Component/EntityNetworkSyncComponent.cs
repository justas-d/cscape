using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Network.Entity.Segment;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{

    [RequiresComponent(typeof(NetworkingComponent))]
    public abstract class EntityNetworkSyncComponent : EntityComponent
    {
        private List<EntityHandle> SyncEntities { get; }
            = new List<EntityHandle>();

        private List<EntityHandle> InitEntities { get; }
            = new List<EntityHandle>();

        protected NetworkingComponent Network => Parent.Components.AssertGet<NetworkingComponent>();
        
        protected EntityNetworkSyncComponent(
            [NotNull] Game.Entities.Entity parent) : base(parent)
        {

        }

        private void AddEntity([NotNull] EntityHandle ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            InitEntities.Add(ent);
        }

        private void RemoveEntity([NotNull] EntityHandle ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            SyncEntities.Add(ent);
            InitEntities.Add(ent);
        }

        protected abstract bool IsHandleableEntity(EntityHandle h);

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
            var removeList = new List<EntityHandle>();
            var syncSegments = new List<IUpdateSegment>();

            foreach (var handle in SyncEntities)
            {

                if (handle.IsDead())
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

            foreach (var handle in removeList)
            {
                SyncEntities.Remove(handle);
            }

            return syncSegments;
        }

        protected abstract void SetInitialFlags(IUpdateWriter writer, Game.Entities.Entity ent);

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

                init.Add(new InitPlayerSegment(
                    entity.Components.AssertGet<PlayerComponent>(),
                    Parent.Components.AssertGet<PlayerComponent>(),
                    needsUpd));

                if (needsUpd)
                {
                    updateSegments.Add(updater);
                }
            }

            InitEntities.Clear();
            return init;
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.NetworkReinitialize:
                {
                    // todo : maybe send remove entity segments when resetting?
                    SyncEntities.Clear();
                    InitEntities.Clear();
                    break;
                }
                case EntityMessage.EventType.NetworkUpdate:
                {
                    Sync();
                    break;
                }
                case EntityMessage.EventType.EntityEnteredViewRange:
                {
                    var h = msg.AsEntityEnteredViewRange();
                    if (IsHandleableEntity(h))
                        AddEntity(h);

                    break;
                }
                case EntityMessage.EventType.EntityLeftViewRange:
                {
                    var h = msg.AsEntityLeftViewRange();
                    if (IsHandleableEntity(h))
                        RemoveEntity(h);

                    break;
                }
            }
        }

        protected abstract void Sync();
    }
}