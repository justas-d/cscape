using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CScape.Data;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network.Sync
{
    public sealed class PlayerUpdateSyncMachine : SyncMachine
    {
        private sealed class PlayerUpdateState : IEquatable<PlayerUpdateState>
        {
            [NotNull]
            public Player Player { get; }

            private Player.UpdateFlags _localFlags;

            private readonly uint _id;

            public PlayerUpdateState([NotNull] Player player)
            {
                Player = player ?? throw new ArgumentNullException(nameof(player));
                _id = player.UniqueEntityId;
            }

            public void SetLocalFlag(Player.UpdateFlags flag)
                => _localFlags |= flag;

            public int NeedsUpdateInt()
                => NeedsUpdate() ? 1 : 0;

            public bool NeedsUpdate()
                => GetCombinedFlags() != 0;

            public Player.UpdateFlags GetCombinedFlags()
                => Player.Flags | _localFlags;

            public void ResetLocalFlags()
                => _localFlags = 0;

            public bool Equals(PlayerUpdateState other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return _id == other._id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is PlayerUpdateState && Equals((PlayerUpdateState) obj);
            }

            public override int GetHashCode()
            {
                return (int) _id;
            }
        }

        public override int Order => Constant.SyncMachineOrder.PlayerUpdate;

        [NotNull] private readonly HashSet<uint> _syncPlayerIds = new HashSet<uint>();
        [NotNull] private ImmutableList<PlayerUpdateState> _syncPlayers = ImmutableList<PlayerUpdateState>.Empty;
        [NotNull] private readonly Queue<Player> _newPlayerQueue = new Queue<Player>();

        [NotNull] private readonly PlayerUpdateState _local;

        public const int Packet = 81;

        public PlayerUpdateSyncMachine(GameServer server, [NotNull] Player localPlayer) : base(server)
        {
            _local = new PlayerUpdateState(localPlayer ?? throw new ArgumentNullException(nameof(localPlayer)));
        }

        public void Clear()
        {
            _syncPlayers = ImmutableList<PlayerUpdateState>.Empty;
            _syncPlayerIds.Clear();
            _newPlayerQueue.Clear();
        }

        public void PushPlayer(Player player)
        {
            if (player.Equals(_local.Player))
                return;

            if (_syncPlayerIds.Contains(player.UniqueEntityId))
                return;

            _newPlayerQueue.Enqueue(player);
        }

        public override void Synchronize(OutBlob stream)
        {
            stream.BeginPacket(Packet);

            stream.BeginBitAccess();

            #region local

            /* Types of updates
           * 
           *  0 (just flag the player)
           *      No further reading. Local player is queued for a flag update.
           *      
           *  1 (walk)
           *      3 bits - movement direction
           *      1 bit  - should queue flag update?
           *      
           *  2 (run)
           *      3 bits - movement direction 1
           *      3 bits - movement direction 2
           *      1 bit  - should queue flag update?
           *      
           *  3 (init)
           *      2 bits - z plane
           *      1 bit  - should set flag to true when calling setPos?
           *      1 bit  - should queue flag update?
           *      7 bits - local y
           *      7 bits - local x
           */
            // 3
            if (_local.Player.NeedsInitWhenLocal)
            {
                stream.WriteBits(1,
                    1); // continue reading? or just "does need updating at all"? If 0, no flag updates will be read for local.

                stream.WriteBits(2, 3); // type

                stream.WriteBits(2, _local.Player.Position.Z); // plane
                stream.WriteBits(1, 1); // todo :  setpos flag
                stream.WriteBits(1, _local.NeedsUpdateInt()); // add to needs updating list
                stream.WriteBits(7, _local.Player.Position.LocalY); // local y
                stream.WriteBits(7, _local.Player.Position.LocalX); // local x
            }
            // 1
            else if (_local.Player.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Walk)
            {
                stream.WriteBits(3, _local.Player.Movement.MoveUpdate.Dir1);
                stream.WriteBits(1, _local.NeedsUpdateInt()); // add to needs updating list
            }
            // 2
            else if (_local.Player.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Run)
            {
                stream.WriteBits(3, _local.Player.Movement.MoveUpdate.Dir1);
                stream.WriteBits(3, _local.Player.Movement.MoveUpdate.Dir2);
                stream.WriteBits(1, _local.NeedsUpdateInt()); // add to needs updating list
            }
            // 0
            else
                stream.WriteBits(1, 0); // 0

            #endregion

            #region _syncPlayers

            var countPos = stream.BitWriteCaret;
            stream.WriteBits(8, 0); // placeholder for the count of existing update ents

            var written = 0;
            foreach (var ent in _syncPlayers)
            {
                // check if the entity is still qualified for updates
                if (ent.Player.IsDestroyed || !_local.Player.CanSee(ent.Player))
                {
                    // send remove payload
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 3); // type
                    // remove internally
                    _syncPlayers = _syncPlayers.Remove(ent);
                    _syncPlayerIds.Remove(ent.Player.UniqueEntityId);
                    continue;
                }

                // run
                if (ent.Player.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Run)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 2); // type
                    stream.WriteBits(3, ent.Player.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(3, ent.Player.Movement.MoveUpdate.Dir2);
                    stream.WriteBits(1, ent.NeedsUpdateInt()); // needs update?
                }
                // walk
                else if (ent.Player.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Walk)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 1); // type
                    stream.WriteBits(3, ent.Player.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(1, ent.NeedsUpdateInt()); // needs update?
                }
                // no pos update, just needs a flag update
                else if (ent.NeedsUpdate())
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

            #region _newPlayerQueue

            while (_newPlayerQueue.Count > 0)
            {
                var player = _newPlayerQueue.Dequeue();
                stream.WriteBits(11, player.Pid); // id

                /*
                 * 1 bit - add to upd list?
                 * todo : 1 bit - setpos flag
                 * 5 bit - y delta from local
                 * 5 bit - x delta from local
                 *
                 *  Since we're adding a new player to the sync list,
                 *  we need to send initial update flags.
                 *  Those would be the facing direction as well as
                 *  appearance.
                 */

                var update = new PlayerUpdateState(player);
                // todo : keep track of appearance stream buffering in the client.

                update.SetLocalFlag(Player.UpdateFlags.Appearance);
                _syncPlayers = _syncPlayers.Add(update);

                stream.WriteBits(1, 1); // has flags. Since this is somebody we haven't seen, write their appearance.
                stream.WriteBits(1, 1); // todo :  setpos flag
                stream.WriteBits(5, player.Position.Y - _local.Player.Position.Y); // ydelta
                stream.WriteBits(5, player.Position.X - _local.Player.Position.X); // xdelta
            }
            stream.WriteBits(11, 2047);

            #endregion

            stream.EndBitAccess();

            WriteFlags(_local, stream);
            foreach (var p in _syncPlayers)
                WriteFlags(p, stream);

            stream.EndPacket();
        }

        private void WriteFlags(PlayerUpdateState upd, Blob stream)
        {
            /*
             * We won't want to sync the chat flag to the local player,
             * so todo check if we're writing flags for the local player before checking the chat flag.
             * if we're writing chat for local, remove chat from flags
             */

            var flags = upd.GetCombinedFlags();

            if (flags == 0)
                return;

            var headerPh = new PlaceholderHandle(stream, 2);

            // write flags
            if (flags.HasFlag(Player.UpdateFlags.Appearance))
                WriteAppearance(upd.Player, stream);

            // todo : the rest of the updates flags.
            // THEY NEED TO FOLLOW A STRICT ORDER

            // write the header
            headerPh.DoWrite(b =>
            {
                stream.Write((byte)flags);
                stream.Write((byte)((short)flags >> 8));
            });

            upd.ResetLocalFlags();
        }

        private void WriteAppearance(Player player, Blob stream)
        {
            const int equipSlotSize = 12;

            const int plrObjMagic = 0x100;
            const int itemMagic = 0x200;

            var sizePh = new PlaceholderHandle(stream, 1);

            stream.Write((byte) player.Appearance.Gender);
            stream.Write((byte) player.Appearance.Overhead);

            /* 
             * todo : some equipped items conflict with body parts 
             * write body model if chest doesn't conceal the body
             * write head model if head item doesn't fully conceal the head.
             * write beard model if head item doesn't fully conceal the head.
             */
            for (var i = 0; i < equipSlotSize; i++)
            {
                if (player.Appearance[i] != null)
                    stream.Write16((short) (player.Appearance[i].Value + plrObjMagic));
                else
                    stream.Write(0);
            }

            stream.Write(player.Appearance.HairColor);
            stream.Write(player.Appearance.TorsoColor);
            stream.Write(player.Appearance.LegColor);
            stream.Write(player.Appearance.FeetColor);
            stream.Write(player.Appearance.SkinColor);

            // player animation indices
            stream.Write16(0x328); // standAnimIndex
            stream.Write16(0x337); // standTurnAnimIndex
            stream.Write16(0x333); // walkAnimIndex
            stream.Write16(0x334); // turn180AnimIndex
            stream.Write16(0x335); // turn90CWAnimIndex
            stream.Write16(0x336); // turn90CCWAnimIndex
            stream.Write16(0x338); // runAnimIndex

            stream.Write64(StringToLong(player.Username));
            stream.Write(128); //cmb
            stream.Write16(0); // ...skill???

            sizePh.WriteSize();

            // todo : proper calculation of cmb lvl
        }

        //smh
        private static long StringToLong(string s)
        {
            var l = 0L;

            foreach (var c in s)
            {
                l *= 37L;
                if (c >= 'A' && c <= 'Z') l += 1 + c - 65;
                else if (c >= 'a' && c <= 'z') l += 1 + c - 97;
                else if (c >= '0' && c <= '9') l += 27 + c - 48;
            }

            while (l % 37L == 0L && l != 0L)
                l /= 37L;

            return l;
        }
    }
}
