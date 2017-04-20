namespace cscape
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

            // for now, simple init-esque updating will do

            // -- local
            stream.WriteBits(1, 1); // should update
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