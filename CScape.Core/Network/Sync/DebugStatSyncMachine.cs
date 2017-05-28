using System;
using CScape.Core.Data;
using CScape.Core.Injection;

namespace CScape.Core.Network.Sync
{
    public sealed class DebugStatSyncMachine : ISyncMachine
    {
        private readonly IMainLoop _loop;
        private bool _prevEnabled;

        public bool IsEnabled { get; set; }

        public int Order => SyncMachineConstants.DebugStat;
        public bool RemoveAfterInitialize { get; } = false;

        public bool NeedsUpdate => (IsEnabled || _prevEnabled);

        public const byte Packet = 2;

        public DebugStatSyncMachine(IServiceProvider services)
        {
            _loop = services.ThrowOrGet<IMainLoop>();
        }

        public void Synchronize(OutBlob stream)
        {
            stream.BeginPacket(Packet);

            stream.Write(IsEnabled ? (byte) 1 : (byte) 0);
            stream.Write16((short) _loop.DeltaTime);
            stream.Write16((short) _loop.TickProcessTime);

            stream.EndPacket();
            _prevEnabled = IsEnabled;
        }

        public void OnReinitialize() {}
    }
}
