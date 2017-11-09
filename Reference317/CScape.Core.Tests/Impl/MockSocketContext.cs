using System;
using System.Net.Sockets;
using CScape.Core.Network;
using CScape.Models.Data;

namespace CScape.Core.Tests.Impl
{
    public class MockSocketContext : ISocketContext
    {
        public static MockSocketContext Instance { get; } = new MockSocketContext();

        private MockSocketContext()
        {
            
        }

        public void Dispose() { }

        public OutBlob OutStream => throw new NotImplementedException();
        public CircularBlob InStream => throw new NotImplementedException();
        public int SignlinkId => throw new NotImplementedException();
        public long DeadForMs => throw new NotImplementedException();
        public bool IsDisposed => throw new NotImplementedException();

        public bool TryReinitialize(Socket socket, int signlinkId) => throw new NotImplementedException();

        public bool CanReinitialize(int signlinkId) => throw new NotImplementedException();

        public bool Update(long deltaTime) => throw new NotImplementedException();

        public bool FlushOutputStream() => throw new NotImplementedException();

        public bool IsConnected() => throw new NotImplementedException();
    }
}