using System;
using System.Collections.Generic;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entities.Message;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Data;
using CScape.Core.Game.Entities.InteractingEntity;
using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public enum FlagType
    {
        Damage,
        FacingDir,
        InteractingEntity,
        DefinitionChange,
        Appearance,
        ChatMessage,
        ForcedMovement,
        ParticleEffect,
        Animation,
        OverheadText
    }

    public sealed class FlagAccumulatorComponent : EntityComponent
    {
        private readonly Dictionary<FlagType, EntityMessage> _flags
            = new Dictionary<FlagType, EntityMessage>();

        public IReadOnlyDictionary<FlagType, EntityMessage> Flags => _flags;

        public MovementMetadata Movement { get; private set; }
        public bool Reinitialize { get; private set; }

        public override int Priority { get; }

        public FlagAccumulatorComponent(Entity parent)
            :base(parent)
        {
        
        }

        private void SetFlag(FlagType type, EntityMessage msg)
        {
            if (_flags.ContainsKey(type))
                _flags[type] = msg;
            else
                _flags.Add(type, msg);
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.NewOverheadText:
                {
                    SetFlag(FlagType.OverheadText, msg);

                    break;
                }
                case EntityMessage.EventType.NewAnimation:
                {
                    SetFlag(FlagType.Animation, msg);

                    break;
                }
                case EntityMessage.EventType.ParticleEffect:
                {
                    SetFlag(FlagType.ParticleEffect, msg);

                    break;
                }
                case EntityMessage.EventType.ForcedMovement:
                {
                    SetFlag(FlagType.ForcedMovement, msg);
                    break;
                }
                case EntityMessage.EventType.ChatMessage:
                {
                    var chat = msg.AsChatMessage();
                    if (chat.IsForced)
                        SetFlag(FlagType.ChatMessage, msg);
                    
                    break;
                }
                case EntityMessage.EventType.AppearanceChanged:
                {
                    SetFlag(FlagType.Appearance, msg);
                    break;
                }
                case EntityMessage.EventType.TookDamage:
                {
                    SetFlag(FlagType.Damage, msg);
                    break;
                }
                case EntityMessage.EventType.NewFacingDirection:
                {
                    SetFlag(FlagType.FacingDir, msg);
                    break;
                }
                case EntityMessage.EventType.NewInteractingEntity:
                {
                    SetFlag(FlagType.InteractingEntity, msg);
                    break;
                }
                case EntityMessage.EventType.DefinitionChange:
                {
                    SetFlag(FlagType.DefinitionChange, msg);
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
                    _flags.Clear();
                    Reinitialize = false;
                    Movement = null;
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

        public LocalPlayerInit([NotNull]PlayerComponent player, bool needsUpdate)
        {
            var local = player.Parent.Components.AssertGet<ClientPositionComponent>();
            _zplane = player.Parent.GetTransform().Z;
            _needsUpdate = needsUpdate;
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

        public NewSyncPlayer(
            [NotNull] PlayerComponent newPlayer, [NotNull] PlayerComponent localPlayer,
            bool needsUpdate)
        {
            _pid = newPlayer.PlayerId;
            _needsUpdate = needsUpdate;
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

    public interface IUpdateWriter : IUpdateSegment
    {
        bool NeedsUpdate();
    }

    [Flags]
    public enum PlayerFlag
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

    [Flags]
    public enum NpcFlag
    {
        Animation = 0x10,
        PrimaryHit = 8,
        ParticleEffect = 0x80,
        InteractingEntity = 0x20,
        Text = 1,
        SecondaryHit = 0x40,
        Definition = 2,
        FacingCoordinate = 4
    }

    public static class FlagHelpers
    {
        public static NpcFlag ToNpc(this FlagType flag)
        {
            switch (flag)
            {
                case FlagType.Damage:
                    return NpcFlag.PrimaryHit;
                        
                case FlagType.FacingDir:
                    return NpcFlag.FacingCoordinate;
                        
                case FlagType.InteractingEntity:
                    return NpcFlag.InteractingEntity;
                    
                case FlagType.DefinitionChange:
                    return NpcFlag.Definition;
                
                case FlagType.ParticleEffect:
                    return NpcFlag.ParticleEffect;
                    
                case FlagType.Animation:
                    return NpcFlag.Animation;
                    
                case FlagType.OverheadText:
                    return NpcFlag.Text;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
            }

        }

        public static PlayerFlag ToPlayer(this FlagType flag)
        {
            switch (flag)
            {
                case FlagType.Damage:
                    return PlayerFlag.PrimaryHit;
                    
                case FlagType.FacingDir: return PlayerFlag.FacingCoordinate;
                    
                case FlagType.InteractingEntity:
                    return PlayerFlag.InteractEnt;
                    
                case FlagType.Appearance:
                    return PlayerFlag.Appearance;
                    
                case FlagType.ChatMessage:
                    return PlayerFlag.Chat;
                    
                case FlagType.ForcedMovement:
                    return PlayerFlag.ForcedMovement;
                    
                case FlagType.ParticleEffect:
                    return PlayerFlag.ParticleEffect;
                    
                case FlagType.Animation:
                    return PlayerFlag.Animation;

                case FlagType.OverheadText:
                    return PlayerFlag.ForcedText;

                default:
                    throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
            }

        }

    }

    public abstract class UpdateWriter : IUpdateWriter
    {
        protected FlagAccumulatorComponent Flags { get; }

        public UpdateWriter(FlagAccumulatorComponent flags)
        {
            Flags = flags;
        }

        public abstract void Write(OutBlob stream);
        public abstract bool NeedsUpdate();

        protected static void WriteForcedMovement(OutBlob stream, ForcedMovement data)
        {
            stream.Write(data.Start.x);
            stream.Write(data.Start.y);
            stream.Write(data.End.x);
            stream.Write(data.End.y);
            stream.Write(data.Duration.x);
            stream.Write(data.Duration.y);
            stream.Write((byte) data.Direction);
        }

        protected static void WriteParticleEffect(OutBlob stream, ParticleEffect effect)
        {
            stream.Write16(effect.Id);
            stream.Write16(effect.Height);
            stream.Write16(effect.Delay);
        }

        protected static void WriteAnimation(OutBlob stream, Animation anim)
        {
            stream.Write16(anim.Id);
            stream.Write(anim.Delay);
        }

        protected static void WriteForcedText(OutBlob stream, string text)
        {
            stream.WriteString(text ?? "");
        }

        protected static void WritePlayerChat(OutBlob stream, ChatMessage msg)
        {
            stream.Write((byte)msg.Color);
            stream.Write((byte)msg.Effects);
            stream.Write((byte)msg.Title);
            stream.WriteString(msg.Message);
        }

        protected static void WriteInteractingEntity(OutBlob stream, IInteractingEntity ent)
        {
            stream.Write16(ent.Id);
        }

        protected static void WritePlayerAppearance(OutBlob stream)
        {
            TODO
            // TODO : WritePlayerAppearance
            throw new NotImplementedException();
        }

        protected static void WriteFacingCoordinate(OutBlob stream, IFacingData facing)
        {
            stream.Write16(facing.SyncX);
            stream.Write16(facing.SyncY);
        }

        protected static void WriteDamage(OutBlob stream, HitData hit)
        {
            stream.Write(hit.Damage);
            stream.Write((byte)hit.Type);
            stream.Write(hit.CurrentHealth);
            stream.Write(hit.MaxHealth);
        }
    }

    public sealed class PlayerUpdateWriter : UpdateWriter
    {
        private PlayerFlag GetHeader()
        {
            PlayerFlag retval = 0;

            foreach (var kvp in Flags.Flags)
            {
                retval |= kvp.Key.ToPlayer();
            }

            return retval;
        }

        public override bool NeedsUpdate()
        {
            return Flags.Flags.Any();
        }

        public override void Write(OutBlob stream)
        {
            var header = GetHeader();
            if (header != 0)
            {
                if ((header & PlayerFlag.ForcedMovement) != 0)
                {
                    WriteForcedMovement(stream, Flags.Flags[FlagType.ForcedMovement].AsForcedMovement());
                }
                if ((header & PlayerFlag.ParticleEffect) != 0)
                {
                    WriteParticleEffect(stream, Flags.Flags[FlagType.ParticleEffect].AsParticleEffect());
                }
                if ((header & PlayerFlag.Animation) != 0)
                {
                    WriteAnimation(stream, Flags.Flags[FlagType.Animation].AsNewAnimation());
                }
                if ((header & PlayerFlag.ForcedText) != 0)
                {
                    WriteForcedText(stream, Flags.Flags[FlagType.OverheadText].AsNewOverheadText());
                }
                if ((header & PlayerFlag.Chat) != 0)
                {
                    WritePlayerChat(stream, Flags.Flags[FlagType.ChatMessage].AsChatMessage());
                }
                if ((header & PlayerFlag.InteractEnt) != 0)
                {
                    WriteInteractingEntity(stream, Flags.Flags[FlagType.InteractingEntity].AsNewInteractingEntity());
                }
                if ((header & PlayerFlag.Appearance) != 0)
                {
                    WritePlayerAppearance(stream);
                }
                if ((header & PlayerFlag.FacingCoordinate) != 0)
                {
                    WriteFacingCoordinate(stream, Flags.Flags[FlagType.FacingDir].AsNewFacingDirection());
                }
                if ((header & PlayerFlag.PrimaryHit) != 0 ||
                    (header & PlayerFlag.SecondaryHit) != 0)
                {
                    WriteDamage(stream, Flags.Flags[FlagType.Damage].AsTookDamage());
                }
            }
        }

        public PlayerUpdateWriter(FlagAccumulatorComponent flags) : base(flags)
        {
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

        private readonly HashSet<EntityHandle> _initEntities
            = new HashSet<EntityHandle>();

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

            IUpdateSegment CommonSegmentResolve(
                bool needsUpdate, FlagAccumulatorComponent flags)
            {
                if (flags.Movement != null)
                {
                    if (flags.Movement.IsWalking)
                    {
                        return new EntityMovementWalk(
                            flags.Movement.Dir1.Direction, needsUpdate);
                    }
                    else
                    {
                        return new EntityMovementRun(
                            flags.Movement.Dir1.Direction,
                            flags.Movement.Dir2.Direction,
                            needsUpdate);
                    }
                }
                if (needsUpdate)
                {
                    return OnlyNeedsUpdate.Instance;
                }

                return NoUpdate.Instance;
            }

            /* Local */
            IUpdateSegment local;
            {
                var flags = Parent.Components.AssertGet<FlagAccumulatorComponent>();
                var updater = new PlayerUpdateWriter(flags);
                var needsUpdate = updater.NeedsUpdate();

                if (flags.Reinitialize)
                {
                    local = new LocalPlayerInit(
                        Parent.Components.AssertGet<PlayerComponent>(),
                        needsUpdate);
                }
                else
                    local = CommonSegmentResolve(needsUpdate, flags);

                if (needsUpdate)
                    updates.Add(updater);
            }
            
            /* Sync */

            /* Initialize */

            /* Update */

            /* Send packet */
            Parent.Components.AssertGet<NetworkingComponent>().SendPacket(
                new PlayerUpdatePacket(
                    local));
        }

    }
}