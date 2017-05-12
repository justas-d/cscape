using System;
using CScape.Core.Data;
using CScape.Core.Injection;

namespace CScape.Core.Network.Sync
{
    public sealed class DebugStatSyncMachine : SyncMachine
    {
        private readonly IMainLoop _loop;
        private bool _prevEnabled;
        public bool IsEnabled { get; set; }

        public const byte Packet = 2;

        public DebugStatSyncMachine(IServiceProvider services)
        {
            _loop = services.ThrowOrGet<IMainLoop>();
        }

        public override int Order => SyncMachineConstants.DebugStat;

        public override void Synchronize(OutBlob stream)
        {
            if (IsEnabled || _prevEnabled)
            {
                stream.BeginPacket(Packet);

                stream.Write(IsEnabled ? (byte)1 : (byte)0);
                stream.Write16((short) _loop.DeltaTime);
                stream.Write16((short) _loop.TickProcessTime);

                stream.EndPacket();
                _prevEnabled = IsEnabled;
            }
        }
    }
}
