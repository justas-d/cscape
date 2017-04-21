using System.Net;

namespace CScape
{
    public interface IGameServerConfig
    {
        int MaxPlayers { get; }
        int Revision { get; }
        string Version { get; }

        EndPoint ListenEndPoint { get; }
        int Backlog { get; }

        string PrivateLoginKeyDir { get; }
    }
}