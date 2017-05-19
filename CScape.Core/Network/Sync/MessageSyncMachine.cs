using System.Collections.Generic;
using CScape.Core.Data;

namespace CScape.Core.Network.Sync
{
    public sealed class MessageSyncMachine : ISyncMachine
    {
        public int Order => SyncMachineConstants.Message;
        public bool RemoveAfterInitialize { get; } = false;

        private readonly Queue<IPacket> _msgs = new Queue<IPacket>();

        public void Enqueue(IPacket msg) 
            => _msgs.Enqueue(msg);

        public void Synchronize(OutBlob stream)
        {
            while (_msgs.Count > 0)
                _msgs.Dequeue().Send(stream);
        }

        public void OnReinitialize()
        {
        }
    }
}
