using System.Net;
using JetBrains.Annotations;

namespace CScape.Models
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
        int EntityGcInternalMs { get; }

        [NotNull]
        string PrivateLoginKeyDir { get; }
        string Greeting { get; }
    }
}