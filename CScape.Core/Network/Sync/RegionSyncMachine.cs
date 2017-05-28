using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Sync
{
    public class RegionSyncMachine : ISyncMachine
    {
        private readonly Player _player;
        public int Order => SyncMachineConstants.Region;
        public bool RemoveAfterInitialize { get; } = false;

        // send region init if regions changed
        public bool NeedsUpdate =>
            (_oldX == Pos.ClientRegion.x && _oldY == Pos.ClientRegion.y)
            || _forceUpdate;

        private IClientTransform Pos => _player.ClientTransform;
        private bool _forceUpdate;

        private int _oldX;
        private int _oldY;

        public const int Packet = 73;

        public RegionSyncMachine(Player player)
        {
            _player = player;
        }

        public void Synchronize(OutBlob stream)
        {
            _player.DebugMsg($"Sync region: {Pos.ClientRegion.x} + 6 {Pos.ClientRegion.y} + 6", ref _player.DebugRegion);

            stream.BeginPacket(Packet);
            stream.Write16((short)(Pos.ClientRegion.x + 6));
            stream.Write16((short)(Pos.ClientRegion.y + 6));
            stream.EndPacket();

            _oldX = Pos.ClientRegion.x;
            _oldY = Pos.ClientRegion.y;
            _forceUpdate = false;
        }

        public void OnReinitialize() => _forceUpdate = true;
    }
}