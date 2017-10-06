using System;
using System.Collections.Generic;
using CScape.Core.Game.Entities.InteractingEntity;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entities.Message;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class FlagAccumulatorComponent : EntityComponent
    {
        public HitData Damage { get; private set; }
        public (int x, int y)? FacingDir { get; private set; }
        public IInteractingEntity InteractingEntity { get; private set; }
        public int? DefinitionChange { get; private set; }
        public MovementMetadata Movement { get; private set; }
        public bool Appearance { get; private set; }
        public ChatMessage ChatMessage { get; private set; }
        public ForcedMovement ForcedMovement { get; private set; }
        public ParticleEffect ParticleEffect{ get; private set; }
        public Animation Animation { get; private set; }
        public string OverheadText { get; private set; }

        public bool Reinitialize { get; private set; }
        

        public override int Priority { get; }

        public FlagAccumulatorComponent(Entity parent)
            :base(parent)
        {
        
        }
       
        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.NewOverheadText:
                {
                    OverheadText = msg.AsNewOverheadText();
                    break;
                }
                case EntityMessage.EventType.NewAnimation:
                {
                    Animation = msg.AsNewAnimation();
                    break;
                }
                case EntityMessage.EventType.ParticleEffect:
                {
                    ParticleEffect = msg.AsParticleEffect();
                    break;
                }
                case EntityMessage.EventType.ForcedMovement:
                {
                    ForcedMovement = msg.AsForcedMovement();
                    break;
                }
                case EntityMessage.EventType.ChatMessage:
                {
                    ChatMessage = msg.AsChatMessage();
                    break;
                }
                case EntityMessage.EventType.AppearanceChanged:
                {
                    Appearance = true;
                    break;
                }
                case EntityMessage.EventType.TookDamage:
                {
                    Damage = msg.AsTookDamage();
                    break;
                }
                case EntityMessage.EventType.NewFacingDirection:
                {
                    FacingDir = msg.AsNewFacingDirection();
                    break;
                }
                case EntityMessage.EventType.NewInteractingEntity:
                {
                    InteractingEntity = msg.AsNewInteractingEntity();
                    break;
                }
                case EntityMessage.EventType.DefinitionChange:
                {
                    DefinitionChange = msg.AsDefinitionChange();
                    break;
                }
                case EntityMessage.EventType.Move:
                {
                    Movement = msg.AsMove();
                    break;
                }
                case EntityMessage.EventType.Teleport:
                {
                    Reinitialize = true;

                    break;
                }
                case EntityMessage.EventType.FrameEnd:
                {
                    Damage = null;
                    FacingDir = null;
                    InteractingEntity = null;
                    DefinitionChange = null;
                    Reinitialize = false;
                    Movement = null;
                    Appearance = false;
                    ChatMessage = null;
                    ForcedMovement = null;
                    ParticleEffect = null;
                    Animation = null;
                    OverheadText = null;
                    break;
                }
            }
        }
    }

    public interface IUpdateSegment
    {
        void Write(OutBlob stream);
    }

    public sealed class LocalPlayerInit : IUpdateSegment
    {
        private readonly int _zplane;
        private readonly bool _needsUpdate;
        private readonly int _localx;
        private readonly int _localy;

        public LocalPlayerInit([NotNull]PlayerComponent player)
        {
            var local = player.Parent.Components.AssertGet<ClientPositionComponent>();
            _zplane = player.Parent.GetTransform().Z;
            _needsUpdate = player.Parent.Components.AssertGet<FlagAccumulatorComponent>().NeedsUpdate;
            _localx = local.Local.x;
            _localy = local.Local.y;
        }

        public LocalPlayerInit(int zplane, bool needsUpdate, int localx, int localy)
        {
            _zplane = zplane;
            _needsUpdate = needsUpdate;
            _localx = localx;
            _localy = localy;
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // continue reading?
            stream.WriteBits(2, 3); // type

            stream.WriteBits(2, _zplane); // plane
            stream.WriteBits(1, 1); // todo :  setpos flag
            stream.WriteBits(1, _needsUpdate ? 1 : 0); // add to needs updating list

            stream.WriteBits(7, _localy); // local y
            stream.WriteBits(7, _localx); // local x
        }
    }

    public sealed class EntityMovementWalk : IUpdateSegment
    {
        private readonly Direction _dir;
        private readonly bool _needsUpdate;

        public EntityMovementWalk(Direction dir, bool needsUpdate)
        {
            Debug.Assert(dir != Direction.None);
            _dir = dir;
            _needsUpdate = needsUpdate;
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // continue reading?
            stream.WriteBits(2, 1); // type

            stream.WriteBits(3, (byte)_dir);

            stream.WriteBits(1, _needsUpdate ? 1 : 0); // add to needs updating list
        }
    }

    public sealed class EntityMovementRun : IUpdateSegment
    {
        private readonly Direction _dir1;
        private readonly Direction _dir2;
        private readonly bool _needsUpdate;

        public EntityMovementRun(Direction dir1, Direction dir2, bool needsUpdate)
        {
            Debug.Assert(dir1 != Direction.None);
            Debug.Assert(dir2 != Direction.None);
            _dir1 = dir1;
            _dir2 = dir2;
            _needsUpdate = needsUpdate;
        }


        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // continue reading?
            stream.WriteBits(2, 2); // type

            stream.WriteBits(3, (byte)_dir1);
            stream.WriteBits(3, (byte)_dir2);

            stream.WriteBits(1, _needsUpdate ? 1 : 0); // add to needs updating list
        }
    }

    public sealed class OnlyNeedsUpdate : IUpdateSegment
    {
        public static OnlyNeedsUpdate Instance { get; } = new OnlyNeedsUpdate();

        private OnlyNeedsUpdate()
        {
            
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // continue reading?
            stream.WriteBits(2, 0); // type
        }
    }

    public sealed class NoUpdate : IUpdateSegment
    {
        public static NoUpdate Instance { get; } = new NoUpdate();

        private NoUpdate()
        {
            
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 0); // continue reading?
        }
    }

    public sealed class RemoveEntity : IUpdateSegment
    {
        public static RemoveEntity Instance { get; } = new RemoveEntity();

        private RemoveEntity()
        {

        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // is not noop?
            stream.WriteBits(2, 3); // type
        }
    }

    public sealed class NewSyncPlayer : IUpdateSegment
    {
        private readonly int _pid;
        private readonly bool _needsUpdate;
        private readonly int _xdelta;
        private readonly int _ydelta;

        public NewSyncPlayer([NotNull] PlayerComponent newPlayer, [NotNull] PlayerComponent localPlayer)
        {
            _pid = newPlayer.PlayerId;
            _needsUpdate = newPlayer.Parent.Components.AssertGet<FlagAccumulatorComponent>().NeedsUpdate;
            _xdelta = newPlayer.Parent.GetTransform().X - localPlayer.Parent.GetTransform().X;
            _xdelta = newPlayer.Parent.GetTransform().Y - localPlayer.Parent.GetTransform().Y;
        }

        public NewSyncPlayer(int pid, bool needsUpdate, int xdelta, int ydelta)
        {
            _pid = pid;
            _needsUpdate = needsUpdate;
            _xdelta = xdelta;
            _ydelta = ydelta;
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(11, _pid); // id
            stream.WriteBits(1, _needsUpdate ? 1 : 0); // needs update?
            stream.WriteBits(1, 1); // todo :  setpos flag
            stream.WriteBits(5, _ydelta); // ydelta
            stream.WriteBits(5, _xdelta); // xdelta            
        }
    }

    public interface IUpdateFlagSegment
    {
        FlagAccumulatorComponent Flags { get; }

        void Write(OutBlob stream);
        bool NeedsUpdate();
    }


    public sealed class LocalPlayerFlags : IUpdateFlagSegment
    {
        public FlagAccumulatorComponent Flags { get; }

        public LocalPlayerFlags([NotNull] PlayerComponent player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            Flags = player.Parent.Components.AssertGet<FlagAccumulatorComponent>();
        }

        public void Write(OutBlob stream)
        {
            throw new NotImplementedException();
        }

        public bool NeedsUpdate()
        {
            if (Flags.Damage != null) return true;
            if (Flags.FacingDir != null) return true;
            if (Flags.InteractingEntity != null) return true;
            if (Flags.Movement != null) return true;
            if (Flags.Appearance) return true;
            if (Flags.ChatMessage != null) return true;
            if (Flags.ForcedMovement != null) return true;
            if (Flags.ParticleEffect != null) return true;
            if (Flags.Animation != null) return true;
            if (Flags.OverheadText != null) return true;

            return false;
        }
    }
    
    public sealed class PlayerUpdatePacket : IUpdateSegment
    {
        [NotNull] private readonly IUpdateSegment _localSegment;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _syncSegments;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _initializeSegments;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _flagSegments;

        public const int Id = 81;

        public PlayerUpdatePacket(
            [NotNull] IUpdateSegment localSegment,
            [NotNull] IEnumerable<IUpdateSegment> syncSegments,
            [NotNull] IEnumerable<IUpdateSegment> initializeSegments,
            [NotNull] IEnumerable<IUpdateSegment> flagSegments)
        {
            _localSegment = localSegment;
            _syncSegments = syncSegments;
            _initializeSegments = initializeSegments;
            _flagSegments = flagSegments;
        }

        public void Write(OutBlob stream)
        { 
            stream.BeginPacket(Id);

            stream.BeginBitAccess();

            _localSegment.Write(stream);

            stream.WriteBits(8, _syncSegments.Count());

            foreach (var segment in _syncSegments)
                segment.Write(stream);

            foreach(var init in _initializeSegments)
                init.Write(stream);

            if (_flagSegments.Any())
            {
                stream.WriteBits(11, 2047);
                stream.EndBitAccess();

                foreach (var flag in _flagSegments)
                    flag.Write(stream);
            }

            stream.EndPacket();
        }
    }

    public sealed class LocalPlayer : IUpdateSegment
    {
        public void Write(OutBlob stream)
        {
            
        }
    }



    /// <summary>
    /// Responsible for syncing every visible player entity to the network.
    /// </summary>
    [RequiresComponent(typeof(NetworkingComponent))]
    [RequiresComponent(typeof(PlayerComponent))]
    public sealed class PlayerNetworkSyncComponent: EntityComponent
    {
        public override int Priority { get; } = ComponentConstants.PriorityPlayerUpdate;

        private HashSet<EntityHandle> _syncEntities = 
            new HashSet<EntityHandle>();

        private List<EntityMessage> _initEntities
            = new List<EntityMessage>();

        public PlayerNetworkSyncComponent(Entity parent)
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
                    var p = GetPlayer(msg.AsEntityEnteredViewRange());
                    if (p == null)
                        break;

                    AddPlayer(p);
                    break;
                }
                case EntityMessage.EventType.EntityLeftViewRange:
                {
                    var p = GetPlayer(msg.AsEntityLeftViewRange());
                    if (p == null)
                        break;

                    RemovePlayer(p);
                    break;
                }
            }
        }

        private void AddPlayer([NotNull] PlayerComponent ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));
        }

        private void RemovePlayer([NotNull] PlayerComponent ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));
        }
        private void Sync()
        {
            void AddToUpdateQueueIfNeeded(FlagAccumulatorComponent flags)
            {
                if (!flags.NeedsUpdate) return;

                // TODO : AddToUpdateQueueIfNeeded(flags);
            }

            IUpdateSegment CommonSegmentResolve(FlagAccumulatorComponent flags)
            {
                if (flags.Movement != null)
                {
                    if (flags.Movement.IsWalking)
                    {
                        return new EntityMovementWalk(
                            flags.Movement.Dir1.Direction, flags.NeedsUpdate);
                    }
                    else
                    {
                        return new EntityMovementRun(
                            flags.Movement.Dir1.Direction,
                            flags.Movement.Dir2.Direction,
                            flags.NeedsUpdate);
                    }
                }
                if (flags.NeedsUpdate)
                {

                    return OnlyNeedsUpdate.Instance;
                }

                return NoUpdate.Instance;
            }

            /* Local */
            IUpdateSegment local;
            {
                var flags = Parent.Components.AssertGet<FlagAccumulatorComponent>();
                if (flags.Reinitialize)
                {
                    AddToUpdateQueueIfNeeded(flags);
                    local = new LocalPlayerInit(Parent.Components.AssertGet<PlayerComponent>());
                }
                else
                    local = CommonSegmentResolve(flags);

                if(flags.NeedsUpdate)
            }
            
            /* Sync */

            /* Initialize */

            /* Update */
        }

    }

    public sealed class UpdateWriter
    {
        enum Flags : int
        {
            ForcedMovement = 0x400,
            ParticleEffect = 0x100,
            Animation = 8,
            ForcedText = 4,
            Chat = 0x80,
            InteractEnt = 0x1,
            Appearance = 0x10,
            FacingCoordinate = 0x2,
            PrimaryHit = 0x20,
            SecondaryHit = 0x200,
        }

        public void SetFlag(int flag)
        {
            
        }

        public void Write(OutBlob stream)
        {
            
        }
    }

}