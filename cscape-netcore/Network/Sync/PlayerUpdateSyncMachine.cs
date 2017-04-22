using System.Collections.Generic;
using System.Diagnostics;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network.Sync
{
    public sealed class PlayerUpdateSyncMachine : SyncMachine
    {
        // todo : once PlayerUpdateSyncMachine is done, remove boilerplate
        private sealed class MessageBuilder
        {
            [NotNull]
            private Player _local;
            private bool _isLocalNew;

            private readonly HashSet<Player> _syncPlayers = new HashSet<Player>();
            private readonly Queue<Player> _newPlayers = new Queue<Player>();

            public void SetLocalPlayer(Player player)
            {
                _local = player;
                _isLocalNew = true;
            }

            public void AddNewPlayer(Player player)
            {
                Debug.Assert(!_syncPlayers.Contains(player));
                Debug.Assert(!player.Equals(_local));
                _newPlayers.Enqueue(player);    
            }

            public void WriteLocal(Blob stream)
            {
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

                // todo : movement updates in WriteLocal
                if (_isLocalNew)
                {
                    stream.WriteBits(1, 1); // continue reading? or just "does need updating at all"? If 0, no flag updates will be read for local.

                    // for now, simple init-spam updating will do

                    stream.WriteBits(2, 3); // type

                    stream.WriteBits(2, _local.Position.Z); // plane
                    stream.WriteBits(1, 1); // setPos flag
                    stream.WriteBits(1, 1); // add to local list
                    stream.WriteBits(7, _local.Position.LocalY); // local y
                    stream.WriteBits(7, _local.Position.LocalX); // local x
                }
                else
                    stream.WriteBits(1, 0);
            }

            public void WriteExistingPlayers(Blob stream)
            {
                // todo : WriteExistingPlayers
                // do range checking here as well, delete if out of range.
                stream.WriteBits(8, 0); // count of existing update players
            }

            public void WriteNewPlayers(Blob stream)
            {
                // todo : WriteNewPlayers
                // no range checking needed
                stream.WriteBits(11, 2047); // index (in this case 2047 is the break flag)
            }

            public void WriteFlags(Blob stream)
            {
                // todo : update flag blocks
                stream.Write(0);
            }

            public void Clear(Player newLocal)
            {
                _newPlayers.Clear();
                _syncPlayers.Clear();
                SetLocalPlayer(newLocal);
            }
        }

        private readonly MessageBuilder _builder = new MessageBuilder();
        public const int Packet = 81;

        public PlayerUpdateSyncMachine(GameServer server) : base(server)
        {
        }

        public void PushPlayer(Player player, bool isLocal)
        {
            if (isLocal)
                _builder.SetLocalPlayer(player);
            else
                _builder.AddNewPlayer(player);
        }

        public void Clear(Player newLocal) 
            => _builder.Clear(newLocal);

        public override int Order => Constant.SyncMachineOrder.PlayerUpdate;

        public override void Synchronize(OutBlob stream)
        {
            // todo : implement all types of player updating
            stream.BeginPacket(Packet);

            stream.BeginBitAccess();

            _builder.WriteLocal(stream);
            _builder.WriteExistingPlayers(stream);
            _builder.WriteNewPlayers(stream);

            stream.EndBitAccess();


            _builder.WriteFlags(stream);

            stream.EndPacket();
        }
    }
}