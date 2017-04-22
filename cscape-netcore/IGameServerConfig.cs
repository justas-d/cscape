using System.Net;
using JetBrains.Annotations;

namespace CScape
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

        [NotNull]
        string PrivateLoginKeyDir { get; }
        string Greeting { get; }
    }
}