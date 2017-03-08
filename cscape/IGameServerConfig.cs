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

    public sealed class PlayerLookupResult
    {
        public enum StatusType
        {
            Success,
            BadPassword,
            NoUserFound
        }

        public StatusType Status { get; }
        [CanBeNull]
        public IPlayerSaveData Data { get; }

        public PlayerLookupResult(StatusType status, [CanBeNull] IPlayerSaveData data)
        {
            if (!Enum.IsDefined(typeof(StatusType), status))
                throw new InvalidEnumArgumentException(nameof(status), (int) status, typeof(StatusType));

            Status = status;
            Data = data;
        }

        public static PlayerLookupResult BadPassword { get; } = new PlayerLookupResult(StatusType.BadPassword, null);
        public static PlayerLookupResult NoUserFound { get; } = new PlayerLookupResult(StatusType.NoUserFound, null);
    }

    public interface IPlayerDatabase
    {
        Task<bool> UserExists(string username);
        Task<PlayerLookupResult> Load(string username, string passwordHash);
        Task Save(Player player);
    }

    public interface IPlayerSaveData
    {
        int Id { get; }
        string PasswordHash { get; }
        string Username { get; }
    }
}