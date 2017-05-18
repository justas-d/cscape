using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    public sealed class PlayerUpdateSyncMachine : ISyncMachine
    {
        private sealed class PlayerUpdateState : IEquatable<PlayerUpdateState>
        {
            public bool IsLocal { get; }
            public bool IsNew { get; set; } = true;

            [NotNull]
            public Player Player { get; }

            private Player.UpdateFlags _localFlags;

            private readonly uint _id;

            public PlayerUpdateState([NotNull] Player player, bool isLocal)
            {
                IsLocal = isLocal;
                Player = player ?? throw new ArgumentNullException(nameof(player));
                _id = player.UniqueEntityId;
            }

            public void SetLocalFlag(Player.UpdateFlags flag)
                => _localFlags |= flag;

            private Player.UpdateFlags? _cCombined = null;

            public Player.UpdateFlags GetCombinedFlags()
            {
                if (_cCombined == null)
                {
                    // todo : optimization : check for Player.PersistFlags flags which sync the same state that has already been synced
                    var ret = Player.TickFlags | _localFlags | Player.PersistFlags;

                    if (ret != 0)
                    {
                        if (IsLocal)
                        {
                            if (ret.HasFlag(Player.UpdateFlags.Chat) &&
                                (Player.LastChatMessage == null || !Player.LastChatMessage.IsForced))
                                ret &= ~Player.UpdateFlags.Chat;
                        }
                    }
                    _cCombined = ret;
                }

                return _cCombined.Value;
            }

            public void PostUpdate()
            {
                _cCombined = null;
                _localFlags = 0;
            }

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

            public override string ToString()
            {
                return Player + $" IsNew: {IsNew} IsLocal: {IsLocal}";
            }
        }

        public int Order => SyncMachineConstants.PlayerUpdate;

        [NotNull] private readonly HashSet<uint> _syncPlayerIds = new HashSet<uint>();
        [NotNull] private ImmutableList<PlayerUpdateState> _syncPlayers = ImmutableList<PlayerUpdateState>.Empty;
        [NotNull] private readonly HashSet<PlayerUpdateState> _initQueue = new HashSet<PlayerUpdateState>();

        private readonly HashSet<uint> _outsideRemoveQueue = new HashSet<uint>();

        [NotNull] private readonly PlayerUpdateState _local;

        public const int Packet = 81;

        public PlayerUpdateSyncMachine([NotNull] Player localPlayer)
        {
            _local = new PlayerUpdateState(localPlayer ?? throw new ArgumentNullException(nameof(localPlayer)), true);
        }

        public void Clear()
        {
            _syncPlayers = ImmutableList<PlayerUpdateState>.Empty;
            _syncPlayerIds.Clear();
            _initQueue.Clear();
        }

        public void PushPlayer(Player player)
        {
            if (player.IsDestroyed)
                return;

            if (player.Equals(_local.Player))
                return;

            if (_syncPlayerIds.Contains(player.UniqueEntityId))
                return;
                
            _initQueue.Add(new PlayerUpdateState(player, false));
            _outsideRemoveQueue.Remove(player.UniqueEntityId);

            _local.Player.Log.Debug(this, $"{_local.Player.Username}: ADD (remove) {player.Username}");
        }

        private void RemoveState(PlayerUpdateState upd)
        {
            _syncPlayers = _syncPlayers.Remove(upd);
            _syncPlayerIds.Remove(upd.Player.UniqueEntityId);
            _outsideRemoveQueue.Remove(upd.Player.UniqueEntityId);

            _local.Player.Log.Debug(this, $"{_local.Player.Username}: remove {upd.Player.Username}");
        }

        public void Remove(Player p)
        {
            _outsideRemoveQueue.Add(p.UniqueEntityId);

            _local.Player.Log.Debug(this, $"{_local.Player.Username}: queue {p.Username}");
        } 

        // player should not be modified when updating.
        public void Synchronize(OutBlob stream)
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

            var willWriteUpdates = false;
            int NeedsUpdates(PlayerUpdateState state)
            {
                var ret = state.GetCombinedFlags() != 0;
                if (ret)
                {
                    willWriteUpdates = true;
                    return 1;
                }
                return 0;
            }

            if (_local.Player.NeedsPositionInit)
            {
                stream.WriteBits(1, 1); // continue reading?
                stream.WriteBits(2, 3); // type

                stream.WriteBits(2, _local.Player.Transform.Z); // plane
                stream.WriteBits(1, 1); // todo :  setpos flag
                stream.WriteBits(1, NeedsUpdates(_local)); // add to needs updating list

                stream.WriteBits(7, _local.Player.ClientTransform.Local.y); // local y
                stream.WriteBits(7, _local.Player.ClientTransform.Local.x); // local x
            }
            // 1
            else if (_local.Player.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Walk)
            {
                stream.WriteBits(1, 1); // continue reading?
                stream.WriteBits(2, 1); // type

                stream.WriteBits(3, _local.Player.Movement.MoveUpdate.Dir1);
                stream.WriteBits(1, NeedsUpdates(_local)); // add to needs updating list
            }
            // 2
            else if (_local.Player.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Run)
            {
                stream.WriteBits(1, 1); // continue reading?
                stream.WriteBits(2, 2); // type

                stream.WriteBits(3, _local.Player.Movement.MoveUpdate.Dir1);
                stream.WriteBits(3, _local.Player.Movement.MoveUpdate.Dir2);
                stream.WriteBits(1, NeedsUpdates(_local)); // add to needs updating list
            }
            // 0
            else if (NeedsUpdates(_local) != 0)
            {
                stream.WriteBits(1, 1); // continue reading?
                stream.WriteBits(2, 0); // type
            }
            else
            {
                stream.WriteBits(1, 0); // continue reading?
            }
                

            #endregion

            #region _syncPlayers

            var beforeCount = _syncPlayers.Count;
            var countPos = stream.BitWriteCaret;
            stream.WriteBits(8, 0); // placeholder for the count of existing update ents

            var written = 0;

            foreach (var ent in _syncPlayers)
            {
                // check if the entity is still qualified for updates
                if (ent.Player.IsDestroyed || _outsideRemoveQueue.Contains(ent.Player.UniqueEntityId))
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 3); // type
                    RemoveState(ent);
                }
                // tp handling
                else if (ent.Player.NeedsPositionInit)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 3); // type

                    _initQueue.Add(ent);
                }

                // run
                else if (ent.Player.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Run)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 2); // type
                    stream.WriteBits(3, ent.Player.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(3, ent.Player.Movement.MoveUpdate.Dir2);
                    stream.WriteBits(1, NeedsUpdates(ent)); // needs update?
                }
                // walk
                else if (ent.Player.Movement.MoveUpdate.Type == MovementController.MoveUpdateData.MoveType.Walk)
                {
                    stream.WriteBits(1, 1); // is not noop?
                    stream.WriteBits(2, 1); // type
                    stream.WriteBits(3, ent.Player.Movement.MoveUpdate.Dir1);
                    stream.WriteBits(1, NeedsUpdates(ent)); // needs update?
                }
                // no pos update, just needs a flag update
                else if (NeedsUpdates(ent) != 0)
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

            foreach (var upd in _initQueue)
            {
                if (upd.IsNew)
                {
                    _syncPlayers = _syncPlayers.Add(upd);
                    _syncPlayerIds.Add(upd.Player.UniqueEntityId);
                }

                stream.WriteBits(11, upd.Player.Pid); // id

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

                // todo : keep track of appearance stream buffering in the client.
                if (upd.IsNew)
                {
                    upd.SetLocalFlag(Player.UpdateFlags.Appearance);

                    if (!upd.IsLocal)
                        upd.SetLocalFlag(Player.UpdateFlags.FacingCoordinate);

                    upd.IsNew = false;
                }

                stream.WriteBits(1, NeedsUpdates(upd) != 0 ? 1 : 0); // needs update?
                stream.WriteBits(1, 1); // todo :  setpos flag
                stream.WriteBits(5, upd.Player.Transform.Y - _local.Player.Transform.Y); // ydelta
                stream.WriteBits(5, upd.Player.Transform.X - _local.Player.Transform.X); // xdelta            
            }
            _initQueue.Clear();

            if (willWriteUpdates)
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
            var flags = upd.GetCombinedFlags();

            if (flags == 0)
            {
                upd.PostUpdate();
                return;
            }

            var headerPh = stream.Placeholder(2);

            // write flags
            if (flags.HasFlag(Player.UpdateFlags.Chat))
            {
                stream.Write(((byte)upd.Player.LastChatMessage.Color));
                stream.Write(((byte)upd.Player.LastChatMessage.Effects));
                stream.Write(upd.Player.TitleIcon);
                stream.WriteString(upd.Player.LastChatMessage.Message);
            }

            if (flags.HasFlag(Player.UpdateFlags.InteractEnt))
            {
                if (upd.Player.InteractingEntity == null)
                    stream.Write16(-1);
                else
                {
                    var interactPlayer = upd.Player.InteractingEntity as Player;
                    if (interactPlayer != null)
                        stream.Write16((short) (interactPlayer.Pid | 0x8000));

                    // todo : write Player.UpdateFlags.InteractEnt for npcs

                }

            }

            if (flags.HasFlag(Player.UpdateFlags.Appearance))
                WriteAppearance(upd, stream);

            if (flags.HasFlag(Player.UpdateFlags.FacingCoordinate))
            {
                if (upd.Player.FacingCoordinate != null)
                {
                    stream.Write16((short) ((upd.Player.FacingCoordinate.Value.x * 2) + 1));
                    stream.Write16((short) ((upd.Player.FacingCoordinate.Value.y * 2) + 1));
                }
                else
                {
                    stream.Write16((short)
                        (((upd.Player.LastMovedDirection.x + upd.Player.Transform.X) * 2) + 1));
                    stream.Write16((short)
                        (((upd.Player.LastMovedDirection.y + upd.Player.Transform.Y) * 2) + 1));
                }
            }

            // todo : the rest of the updates flags.
            // THEY NEED TO FOLLOW A STRICT ORDER

            // write the header
            headerPh.Write(b =>
            {
                stream.Write((byte)flags);
                stream.Write((byte)((short)flags >> 8));
            });

            upd.PostUpdate();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteAppearance(PlayerUpdateState upd, Blob stream)
        {
            // Cache is invalidated only when the invalidation bool flag is set.
            // check if cache is not ok.

            var cache = upd.Player.AppearanceUpdateCache;

            if (upd.Player.IsAppearanceDirty)
            {
                upd.Player.Log.Debug(this, $"Rewriting appearance cache for {upd}");

                upd.Player.IsAppearanceDirty = false;
                cache.ResetWrite();

                const int equipSlotSize = 12;

                const int plrObjMagic = 0x100;
                const int itemMagic = 0x200;

                var sizePh = cache.Placeholder(1);

                cache.Write((byte) upd.Player.Appearance.Gender);
                cache.Write((byte) upd.Player.Appearance.Overhead);

                /* 
                 * todo : some equipped items conflict with body parts 
                 * write body model if chest doesn't conceal the body
                 * write head model if head item doesn't fully conceal the head.
                 * write beard model if head item doesn't fully conceal the head.
                 */
                for (var i = 0; i < equipSlotSize; i++)
                {
                    // todo : write equipment
                    if (upd.Player.Appearance[i] != null)
                        cache.Write16((short) (upd.Player.Appearance[i].Value + plrObjMagic));
                    else
                        cache.Write(0);
                }

                cache.Write(upd.Player.Appearance.HairColor);
                cache.Write(upd.Player.Appearance.TorsoColor);
                cache.Write(upd.Player.Appearance.LegColor);
                cache.Write(upd.Player.Appearance.FeetColor);
                cache.Write(upd.Player.Appearance.SkinColor);

                // upd.Player animation indices
                cache.Write16(0x328); // standAnimIndex
                cache.Write16(0x337); // standTurnAnimIndex
                cache.Write16(0x333); // walkAnimIndex
                cache.Write16(0x334); // turn180AnimIndex
                cache.Write16(0x335); // turn90CWAnimIndex
                cache.Write16(0x336); // turn90CCWAnimIndex
                cache.Write16(0x338); // runAnimIndex

                cache.Write64(Utils.StringToLong(upd.Player.Username));
                cache.Write(3); // todo : cmb
                cache.Write16(0); // ...skill???

                sizePh.WriteSize();

                // todo : proper calculation of cmb lvl
            }

            // cache is ok or we rewrote it. Eitherway, flush it into the stream.
            cache.FlushInto(stream);
        }
    }
}
