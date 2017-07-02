using System.Collections.Generic;
using System.Collections.Immutable;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    public sealed class NpcUpdateSyncMachine : ISyncMachine
    {
        private sealed class NpcUpdateState
        {
            public Npc Npc { get; }
            private Npc.UpdateFlags _localFlags;
            public bool IsNew { get; set; }

            public void SetLocalFlag(Npc.UpdateFlags flag) => _localFlags |= flag;

            public Npc.UpdateFlags GetFlags() => _localFlags | Npc.TickFlags;

            public NpcUpdateState(Npc npc)
            {
                Npc = npc;
                IsNew = true;
            }    
        }

        private bool _ignoreEmpty = false;
        public bool NeedsUpdate => true;

        private readonly Player _local;

        // to avoid pushing duplicate to _syncNpcs. Can't set _syncNpcs to a hashset due to hashsets not guaranteeing order.
        [NotNull] private readonly HashSet<uint> _syncNpcIds = new HashSet<uint>();
        [NotNull] private ImmutableList<NpcUpdateState> _syncNpcs = ImmutableList<NpcUpdateState>.Empty;
        [NotNull] private readonly HashSet<uint> _removeQueue = new HashSet<uint>();

        [NotNull] private readonly HashSet<NpcUpdateState> _initQueue = new HashSet<NpcUpdateState>();

        public const int Packet = 65;
        public int Order => SyncMachineConstants.NpcUpdate;
        public bool RemoveAfterInitialize { get; } = false;

        public NpcUpdateSyncMachine([NotNull] Player local)
        {
            _local = local;
        }

        public void Clear()
        {
            _syncNpcs = ImmutableList<NpcUpdateState>.Empty;
            _syncNpcIds.Clear();
            _initQueue.Clear();
            _ignoreEmpty = true;
        }

        private const int MaxNpcs = 2;
        private int _npcUpdateCount = 0;
        private bool _npcOverflow;

        public void UpdateNpc(Npc npc)
        {
            if (npc.IsDestroyed)
                return;

            if (_npcOverflow)
                return;

            if (_npcUpdateCount++ >= MaxNpcs)
            {
                _npcOverflow = true;
                return;
            }

            if (_syncNpcIds.Contains(npc.UniqueEntityId))
                return;

            _initQueue.Add(new NpcUpdateState(npc));
            _removeQueue.Remove(npc.UniqueEntityId);

            _local.DebugMsg($"(NPC) (remove) push: def{npc.NpcDefinitionId}", ref _local.DebugEntitySync);
        }

        private bool IsEmpty() => (0 >= _initQueue.Count && 0 >= _syncNpcs.Count);

        private void RemoveState(NpcUpdateState state)
        {
            _removeQueue.Remove(state.Npc.UniqueEntityId);

            _syncNpcs = _syncNpcs.Remove(state);
            _initQueue.Remove(state);

            _syncNpcIds.Remove(state.Npc.UniqueEntityId);

            if (IsEmpty())
            {
                // deleting the last npc that we sync will cause the deletion of that entity to not be synced
                // because of IsEmpty being true and Synchronize not doing anything when it is.
                // this works around that.
                _local.DebugMsg($"(NPC) set ignore empty flag", ref _local.DebugEntitySync);
                _ignoreEmpty = true;
            }
        }

        public void Remove(Npc npc)
        {
            _removeQueue.Add(npc.UniqueEntityId);
            _local.DebugMsg($"(NPC) remove: def{npc.NpcDefinitionId}", ref _local.DebugEntitySync);
        }


        public void Synchronize(OutBlob stream)
        {
            // no need to sync if we've got no npcs to sync.
            if (!_ignoreEmpty && IsEmpty()) return;
            _ignoreEmpty = false;

            var willWriteFlags = false;
            int NeedsUpdate(NpcUpdateState state)
            {
                var ret = state.GetFlags() != 0;
                if (ret)
                {
                    willWriteFlags = true;
                    return 1;
                }
                return 0;
            }

            stream.BeginPacket(Packet);
            stream.BeginBitAccess();

            #region existing npcs

            var beforeCount = _syncNpcs.Count;
            var countPos = stream.BitWriteCaret;
            stream.WriteBits(8, 0); // placeholder for the count of existing update ents
            var written = 0;

            foreach (var state in _syncNpcs)
            {
                // check if the entity is still qualified for updates
                if (state.Npc.IsDestroyed || _removeQueue.Contains(state.Npc.UniqueEntityId))
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 3); // type
                    RemoveState(state);
                }
                // tp handling
                else if (state.Npc.NeedsPositionInit)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 3); // type

                    _initQueue.Add(state);
                }

                // run
                else if (state.Npc.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Run)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 2); // type
                    stream.WriteBits(3, state.Npc.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(3, state.Npc.Movement.MoveUpdate.Dir2);
                    stream.WriteBits(1, NeedsUpdate(state)); // needs update?
                }
                // walk
                else if (state.Npc.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Walk)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 1); // type
                    stream.WriteBits(3, state.Npc.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(1, NeedsUpdate(state)); // needs update?
                }
                // no pos update, just needs a flag update
                else if (NeedsUpdate(state) != 0)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 0); // type

                }
                // absolutely no update
                else
                {
                    stream.WriteBits(1, 0); // is not noop?
                }

                written++;
            }
            var actualPos = stream.BitWriteCaret;
            stream.BitWriteCaret = countPos;

            // write the placeholder
            stream.WriteBits(8, written);

            stream.BitWriteCaret = actualPos;

            #endregion

            #region new npcs

            foreach (var state in _initQueue)
            {
                if (state.IsNew)
                {
                    _syncNpcs = _syncNpcs.Add(state);
                    _syncNpcIds.Add(state.Npc.UniqueEntityId);

                    state.IsNew = false;

                    // first sighting flags

                    state.SetLocalFlag(Npc.UpdateFlags.InteractingEntity);
                    state.SetLocalFlag(Npc.UpdateFlags.FacingCoordinate);
                }

                stream.WriteBits(14, state.Npc.UniqueNpcId); // id

                stream.WriteBits(5, state.Npc.Transform.Y - _local.Transform.Y); // ydelta
                stream.WriteBits(5, state.Npc.Transform.X - _local.Transform.X); // xdelta   

                stream.WriteBits(1, 1); // todo :  setpos flag
                stream.WriteBits(12, state.Npc.NpcDefinitionId); // def
                stream.WriteBits(1, NeedsUpdate(state) != 0 ? 1 : 0); // needs update?
            }
            _initQueue.Clear();

            if (willWriteFlags)
                stream.WriteBits(14, 16383);

            #endregion

            stream.EndBitAccess();

            foreach (var state in _syncNpcs)
                WriteFlags(state, stream);

            stream.EndPacket();

            // post
            if (_npcOverflow)
            {
                _local.ViewRange--;
            }
            else if (_npcUpdateCount < MaxNpcs)
            {
                if (_local.ViewRange != Player.MaxViewRange)
                    _local.ViewRange++;
            }

            _npcUpdateCount = 0;
            _npcOverflow = false;
        }

        public void OnReinitialize() { }

        private void WriteFlags(NpcUpdateState state, Blob stream)
        {
            var flags = state.GetFlags();
            if (flags == 0) return;

            // write header
            stream.Write((byte)flags);
            
            if ((flags & Npc.UpdateFlags.Animation) != 0)
            {
                var data = state.Npc.Animation ?? Animation.Reset;
                stream.Write16(data.Id);
                stream.Write(data.Delay);
            }

            if((flags & Npc.UpdateFlags.PrimaryHit) != 0)
                EntityHelper.WriteHitData(stream, state.Npc, false);

            if ((flags & Npc.UpdateFlags.ParticleEffect) != 0)
            {
                if (state.Npc.Effect == null)
                    state.Npc.Effect = ParticleEffect.Stop;

                stream.Write16(state.Npc.Effect.Id);
                stream.Write16(state.Npc.Effect.Height);
                stream.Write16(state.Npc.Effect.Delay);
            }

            if ((flags & Npc.UpdateFlags.InteractingEntity) != 0)
                EntityHelper.WriteInteractingEntityFlag(state.Npc, state.Npc.UniqueNpcId, stream);

            if ((flags & Npc.UpdateFlags.Text) != 0 && state.Npc.LastSentTextMessage != null)
                stream.WriteString(state.Npc.LastSentTextMessage);

            if ((flags & Npc.UpdateFlags.SecondaryHit) != 0)
                EntityHelper.WriteHitData(stream, state.Npc, true);

            if ((flags & Npc.UpdateFlags.Definition) != 0)
                stream.Write16(state.Npc.NpcDefinitionId);

            if((flags & Npc.UpdateFlags.FacingCoordinate) != 0)
                EntityHelper.WriteFacingDirection(state.Npc, state.Npc.FacingCoordinate, stream);
        }
    }
}
