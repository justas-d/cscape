using System.Net;
using CScape.Core.Game;

namespace CScape.Dev.Tests.Impl
{
    public class MockConfig : IGameServerConfig
    {
        public int MaxPlayers { get; } 
        public int Revision { get; } 
        public string Version { get; }
        public EndPoint ListenEndPoint { get; }
        public int Backlog { get; }
        public int SocketSendTimeout { get; }
        public int SocketReceiveTimeout { get; }
        public int TickRate { get; }
        public int AutoSaveIntervalMs { get; }
        public ChatMessage.TextEffect DefaultChatEffect { get; } = ChatMessage.TextEffect.None;
        public ChatMessage.TextColor DefaultChatColor { get; } = ChatMessage.TextColor.Yellow;
        public string PrivateLoginKeyDir { get; }
        public string Greeting { get; }
    }
}
