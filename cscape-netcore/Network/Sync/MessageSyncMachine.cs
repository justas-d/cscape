using System.Collections.Generic;
using CScape.Data;

namespace CScape.Network.Sync
{
    public sealed class MessageSyncMachine : SyncMachine
    {
        public override int Order => Constant.SyncMachineOrder.Message;

        public MessageSyncMachine(GameServer server) : base(server)
        {
        }

        private readonly Queue<IPacket> _msgs = new Queue<IPacket>();

        public void Enqueue(IPacket msg) 
            => _msgs.Enqueue(msg);

        public override void Synchronize(OutBlob stream)
        {
            while (_msgs.Count > 0)
                _msgs.Dequeue().Send(stream);
        }
    }
}
