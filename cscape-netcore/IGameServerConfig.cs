using System;
using System.ComponentModel;
using System.Net;
using JetBrains.Annotations;

namespace cscape
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