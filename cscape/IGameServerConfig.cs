using System.Net;

namespace cscape
{
    public interface IGameServerConfig
    {
        int MaxPlayers { get; }
        string PrivateLoginKeyDir { get; }
        int Revision { get; }
        string Version { get; }

        EndPoint ListenEndPoint { get; }
        int Backlog { get; }
    }
}