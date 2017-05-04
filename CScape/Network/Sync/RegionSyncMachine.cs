using System;
using CScape.Data;
using CScape.Game.Entity;

namespace CScape.Network.Sync
{
    public class RegionSyncMachine : SyncMachine
    {
        private readonly Player _player;
        public override int Order => Constant.SyncMachineOrder.Region;

        /// <summary>
        /// Schedules a forced region update during the next sync round.
        /// </summary>
        public bool ForceUpdate { private get; set; }

        private ITransform Pos => _player.Transform;

        private int _oldX;
        private int _oldY;

        public const int Packet = 73;

        public RegionSyncMachine(GameServer server, Player player) : base(server)
        {
            _player = player;
        }

        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/></exception>
        public override void Synchronize(OutBlob stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // send region init if regions changed
            if ((_oldX == Pos.ClientRegion.x && _oldY == Pos.ClientRegion.y) || ForceUpdate) return;

            _player.DebugMsg($"Sync region: {Pos.ClientRegion.x} + 6 {Pos.ClientRegion.y} + 6", ref _player.DebugRegion);

            stream.BeginPacket(Packet);
            stream.Write16((short)(Pos.ClientRegion.x + 6));
            stream.Write16((short)(Pos.ClientRegion.y + 6));
            stream.EndPacket();

            _oldX = Pos.ClientRegion.x;
            _oldY = Pos.ClientRegion.y;
            ForceUpdate = false;
        }
    }
}