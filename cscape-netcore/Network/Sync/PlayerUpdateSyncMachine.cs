using CScape.Game.Entity;

namespace CScape.Network.Sync
{
    public class PlayerUpdateSyncMachine : SyncMachine
    {
        private PositionController Pos => _player.Position;
        private readonly Player _player;
        public const int PlayerUpdatePacketId = 81;

        public PlayerUpdateSyncMachine(GameServer server, Player player) : base(server)
        {
            _player = player;
        }

        public override void Synchronize(Blob stream)
        {
            BeginPacket(stream, PlayerUpdatePacketId);
            stream.BeginBitAccess();

            // todo : implement all types of player updating

            // for now, simple init-spam updating will do

            // -- local
            stream.WriteBits(1, 1); // continue reading? or just "does need updating at all"? If 0, no flag updates will be read for local.

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
            stream.WriteBits(2, 3); // type

            stream.WriteBits(2, Pos.Z); // plane
            stream.WriteBits(1, 1); // setPos flag
            stream.WriteBits(1, 1); // add to local list
            stream.WriteBits(7, Pos.LocalY); // local y
            stream.WriteBits(7, Pos.LocalY); // local z

            // -- update other existing
            stream.WriteBits(8, 0); // count of existing update players

            // -- add new update players
            stream.WriteBits(11, 2047); // index (in this case 2047 is the break flag)

            stream.EndBitAccess();

            // -- update flags
            // todo : update flag blocks
            // for now, no update flags, not even appearance.
            stream.Write(0);

            EndPacket(stream);
        }
    }
}