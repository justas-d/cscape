using System.Collections.Generic;
using System.Net.Sockets;
using CScape.Core.Data;
using CScape.Core.Injection;
using CScape.Core.Network;
using CScape.Core.Network.Sync;

namespace CScape.Dev.Tests.Impl
{
    public class MockSocketContext : ISocketContext
    {
        public void Dispose() { }

        public OutBlob OutStream { get; }
        public CircularBlob InStream { get; }
        public int SignlinkId { get; }
        public long DeadForMs { get; }
        public bool IsDisposed { get; }
        public IList<ISyncMachine> SyncMachines { get; } = new List<ISyncMachine>();

        public bool TryReinitialize(Socket socket, int signlinkId) => false;

        public bool CanReinitialize(int signlinkId) => false;

        public bool Update(long deltaTime) => true;

        public bool FlushOutputStream() => true;

        public bool IsConnected() => true;

        public void SendPacket(IPacket packet)
        {
        }
    }
}