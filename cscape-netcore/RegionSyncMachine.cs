using System;

namespace cscape
{
    public class RegionSyncMachine : SyncMachine
    {
        private readonly PositionController _pos;

        private int _oldX;
        private int _oldY;

        public const int RegionInitOpcode = 73;

        public RegionSyncMachine(GameServer server, PositionController pos) : base(server)
        {
            _pos = pos;
        }

        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/></exception>
        public override void Synchronize(Blob stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // send region init if regions changed
            if (_oldX == _pos.RegionX && _oldY == _pos.RegionY) return;

            BeginPacket(stream, RegionInitOpcode);
            stream.Write16((short)_pos.RegionX);
            stream.Write16((short)_pos.RegionY);
            EndPacket(stream);

            _oldX = _pos.RegionX;
            _oldY = _pos.RegionY;
        }
    }
}