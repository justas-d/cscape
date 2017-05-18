using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    public sealed class NpcUpdateSyncMachine : ISyncMachine
    {
        private readonly Player _local;
        public int Order => SyncMachineConstants.NpcUpdate;

        [NotNull] private readonly HashSet<uint> _syncNpcIds = new HashSet<uint>();
        [NotNull] private readonly HashSet<Npc> _initQueue = new HashSet<Npc>();
        [NotNull] private readonly HashSet<uint> _newNpcIds = new HashSet<uint>();

        [NotNull] private ImmutableList<Npc> _syncNpcs = ImmutableList<Npc>.Empty;

        public const int Packet = 65;

        public NpcUpdateSyncMachine([NotNull] Player local)
        {
            _local = local;
        }

        public void Clear()
        {
            _syncNpcs = ImmutableList<Npc>.Empty;
            _syncNpcIds.Clear();
            _initQueue.Clear();
        }

        public void PushNpc(Npc npc)
        {
            if (npc.IsDestroyed)
                return;

            if (_syncNpcIds.Contains(npc.UniqueEntityId))
                return;

            _initQueue.Add(npc);
            _newNpcIds.Add(npc.UniqueEntityId);
        }

        public void Remove(Npc npc)
        {
            _syncNpcs = _syncNpcs.Remove(npc);
            _initQueue.Remove(npc);

            _syncNpcIds.Remove(npc.UniqueEntityId);
            _newNpcIds.Remove(npc.UniqueEntityId);
        }

        public void Synchronize(OutBlob stream)
        {
            // no need to sync if we've got no npcs to sync.
            if (0 >= _initQueue.Count && 0 >= _syncNpcs.Count) return;

            var willWriteFlags = false;
            int NeedsUpdate(Npc ent)
            {
                var ret = (ent.TickFlags | ent.PersistFlags) != 0;
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

            foreach (var ent in _syncNpcs)
            {
                // check if the entity is still qualified for updates
                if (ent.IsDestroyed)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 3); // type
                    Remove(ent);
                }
                // tp handling
                else if (ent.NeedsPositionInit)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 3); // type

                    _initQueue.Add(ent);
                }

                // run
                else if (ent.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Run)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 2); // type
                    stream.WriteBits(3, ent.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(3, ent.Movement.MoveUpdate.Dir2);
                    stream.WriteBits(1, NeedsUpdate(ent)); // needs update?
                }
                // walk
                else if (ent.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Walk)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 1); // type
                    stream.WriteBits(3, ent.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(1, NeedsUpdate(ent)); // needs update?
                }
                // no pos update, just needs a flag update
                else if (NeedsUpdate(ent) != 0)
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

            foreach (var ent in _initQueue)
            {
                var isNew = _newNpcIds.Contains(ent.UniqueEntityId);
                if (isNew)
                {
                    _syncNpcs = _syncNpcs.Add(ent);
                    _syncNpcIds.Add(ent.UniqueEntityId);
                    _newNpcIds.Remove(ent.UniqueEntityId);
                }

                stream.WriteBits(14, ent.UniqueNpcId); // id

                stream.WriteBits(5, ent.Transform.Y - _local.Transform.Y); // ydelta
                stream.WriteBits(5, ent.Transform.X - _local.Transform.X); // xdelta   

                stream.WriteBits(1, 1); // todo :  setpos flag
                stream.WriteBits(12, ent.NpcDefinitionId); // def
                stream.WriteBits(1, NeedsUpdate(ent) != 0 ? 1 : 0); // needs update?
            }
            _initQueue.Clear();

            if (willWriteFlags)
                stream.WriteBits(14, 16383);

            #endregion

            stream.EndBitAccess();

            foreach (var ent in _syncNpcs)
                WriteFlags(ent, stream);

            stream.EndPacket();
        }

        private void WriteFlags(Npc ent, Blob stream)
        {
            return;

            var flags = ent.TickFlags | ent.PersistFlags;
            // todo : write npc flags
        }
    }
}
