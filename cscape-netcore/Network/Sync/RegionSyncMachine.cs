using System;
using CScape.Game.Entity;

namespace CScape.Network.Sync
{
    public class RegionSyncMachine : SyncMachine
    {
        public override int Order => Constant.SyncMachineOrder.Region;

        private readonly Transform _pos;

        private int _oldX;
        private int _oldY;

        public const int Packet = 73;

        public RegionSyncMachine(GameServer server, Transform pos) : base(server)
        {
            _pos = pos;
        }

        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/></exception>
        public override void Synchronize(OutBlob stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // send region init if regions changed
            if (_oldX == _pos.RegionX && _oldY == _pos.RegionY) return;

            stream.BeginPacket(Packet);
            stream.Write16((short)_pos.RegionX);
            stream.Write16((short)_pos.RegionY);
            stream.EndPacket();

            _oldX = _pos.RegionX;
            _oldY = _pos.RegionY;
        }
    }
}