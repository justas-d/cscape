using System.Net;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IGameServerConfig
    {
        int MaxPlayers { get; }
        int Revision { get; }
        [NotNull]
        string Version { get; }

        [NotNull]
        EndPoint ListenEndPoint { get; }
        int Backlog { get; }
        int SocketSendTimeout { get; }
        int SocketReceiveTimeout { get; }

        int TickRate { get; }
        int AutoSaveIntervalMs { get; }

        ChatMessage.TextEffect DefaultChatEffect { get; }
        ChatMessage.TextColor DefaultChatColor { get; }

        [NotNull]
        string PrivateLoginKeyDir { get; }
        string Greeting { get; }
    }
}