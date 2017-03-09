using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
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

    public interface IDatabase
    {
        IPacketLengthLookup Packet { get; }
        IPlayerDatabase Player { get; }
    }

    public interface IPlayerDatabase
    {
        Task<bool> UserExists(string username);
        Task<IPlayerSaveData> Load(string username, string password);
        Task Save(Player player);
        Task<IPlayerSaveData> LoadOrCreateNew(string username, string pwd);
    }
}