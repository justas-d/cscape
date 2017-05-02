using System;
using CScape.Data;

namespace CScape.Network.Sync
{
    public sealed class DebugStatSyncMachine : SyncMachine
    {
        private bool _prevEnabled;

        public bool IsEnabled { get; set; }

        public const byte Packet = 2;

        public DebugStatSyncMachine(GameServer server) : base(server)
        {
        }

        public override int Order => Constant.SyncMachineOrder.DebugStat;

        public override void Synchronize(OutBlob stream)
        {
            if (IsEnabled || _prevEnabled)
            {
                stream.BeginPacket(Packet);

                stream.Write(IsEnabled ? (byte)1 : (byte)0);
                stream.Write16((short)Server.Loop.DeltaTime);
                stream.Write16((short)Server.Loop.TickProcessTime);

                stream.EndPacket();
                _prevEnabled = IsEnabled;
            }
        }
    }
}
