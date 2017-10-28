using System.Net;
using CScape.Models;

namespace CScape.Dev.Tests.Impl
{
    public class MockConfig : IGameServerConfig
    {
        public int MaxPlayers => 2000;
        public int MaxNpcs => 16000;
        public int Revision => 317;
        public string Version => "Dev.Tests";
        public EndPoint ListenEndPoint { get; }
        public int Backlog { get; }
        public int SocketSendTimeout { get; }
        public int SocketReceiveTimeout { get; }
        public int TickRate => 600;
        public int EntityGcInternalMs { get; }
        public string PrivateLoginKeyDir { get; }
        public string Greeting { get; }
    }
}
