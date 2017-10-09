using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Network.Entity.Flag;
using CScape.Core.Network.Entity.Segment;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    /// <summary>
    /// Responsible for syncing every visible player entity to the network.
    /// </summary>
    [RequiresComponent(typeof(NetworkingComponent))]
    [RequiresComponent(typeof(PlayerComponent))]
    public sealed class PlayerNetworkSyncComponent: EntityComponent
    {
        public override int Priority { get; } = ComponentConstants.PriorityPlayerUpdate;

        private readonly List<EntityHandle> _syncEntities = 
            new List<EntityHandle>();

        private readonly List<EntityHandle> _initEntities
            = new List<EntityHandle>();

        public PlayerNetworkSyncComponent(Game.Entities.Entity parent)
            :base(parent)
        {
            
        }
        
        public override void ReceiveMessage(EntityMessage msg)
        {
            
            PlayerComponent GetPlayer(EntityHandle h)
            {
                if (h.IsDead())
                    return null;
                var p = h.Get().Components.Get<PlayerComponent>();
                return p;
            }

            switch (msg.Event)
            {
                case EntityMessage.EventType.NetworkUpdate:
                {
                    Sync();
                    break;
                }
                case EntityMessage.EventType.EntityEnteredViewRange:
                {
                    var h = msg.AsEntityEnteredViewRange();
                    if (GetPlayer(h) == null)
                        break;

                    AddPlayer(h);
                    break;
                }
                case EntityMessage.EventType.EntityLeftViewRange:
                {
                    var h = msg.AsEntityLeftViewRange();
                    if (GetPlayer(h) == null)
                        break;

                    RemovePlayer(h);
                    break;
                }
            }
        }

        private void AddPlayer([NotNull] EntityHandle ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            _initEntities.Add(ent);
        }

        private void RemovePlayer([NotNull] EntityHandle ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            _syncEntities.Add(ent);
            _initEntities.Add(ent);
        }

        private void Sync()
        {
            var updates = new List<IUpdateWriter>();
            var syncSegments = new List<IUpdateSegment>();
            var removeList = new HashSet<EntityHandle>();
            var initSegments = new List<IUpdateSegment>();

            IUpdateSegment CommonSegmentResolve(
                bool needsUpdate, FlagAccumulatorComponent flags)
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
                {
                    return OnlyNeedsUpdateSegment.Instance;
                }

                return NoUpdateSegment.Instance;
            }

            /* Local */
            IUpdateSegment local;
            {
                var flags = Parent.Components.AssertGet<FlagAccumulatorComponent>();
                var updater = new LocalUpdateWriter(flags);
                var needsUpdate = updater.NeedsUpdate();

                if (flags.Reinitialize)
                {
                    local = new LocalPlayerInitSegment(
                        Parent.Components.AssertGet<PlayerComponent>(),
                        needsUpdate);
                }
                else
                    local = CommonSegmentResolve(needsUpdate, flags);

                if (needsUpdate)
                    updates.Add(updater);
            }

      
            /* Sync */
            foreach (var handle in _syncEntities)
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
                        _initEntities.Add(handle);
                        removeList.Add(handle);
                    }
                    else
                    {
                        var updater = new PlayerUpdateWriter(flags);
                        var needsUpdate = updater.NeedsUpdate();

                        syncSegments.Add(CommonSegmentResolve(needsUpdate, flags));
                        if (needsUpdate)
                        {
                            updates.Add(updater);
                        }
                    }
                }
            }

            foreach (var handle in removeList)
            {
                _syncEntities.Remove(handle);
            }
            removeList.Clear();

            /* Initialize */
            foreach (var handle in _initEntities.Where(h => !h.IsDead()))
            {
                var entity = handle.Get();
                var flags = entity.Components.AssertGet<FlagAccumulatorComponent>();
                var updater = new PlayerUpdateWriter(flags);

                _syncEntities.Add(handle);
                updater.SetFlag(new  PlayerAppearanceUpdateFlag());
                updater.SetFlag(new FacingCoordinateUpdateFlag(entity.GetTransform().FacingData));
                updater.SetFlag(new InteractingEntityUpdateFlag(entity.GetTransform().InteractingEntity));

                var needsUpd = updater.NeedsUpdate();

                initSegments.Add(new InitPlayerSegment(
                    entity.Components.AssertGet<PlayerComponent>(),
                    Parent.Components.AssertGet<PlayerComponent>(),
                    needsUpd));

                if (needsUpd)
                {
                    updates.Add(updater);
                }
            }

            _initEntities.Clear();

            /* Update */

            /* Send packet */
            Parent.Components.AssertGet<NetworkingComponent>().SendPacket(
                new PlayerUpdatePacket(
                    local,
                    syncSegments,
                    initSegments,
                    updates));
        }

    }
}