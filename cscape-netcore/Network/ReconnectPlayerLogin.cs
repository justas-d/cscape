using System;
using System.Net.Sockets;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network
{
    public class ReconnectPlayerLogin : IPlayerLogin
    {
        public Socket NewConnection { get; }
        public Player Existing { get; }
        public int SignlinkUid { get; }

        public ReconnectPlayerLogin([NotNull] Player existing, [NotNull] Socket newConnection, int signlinkUid)
        {
            NewConnection = newConnection ?? throw new ArgumentNullException(nameof(newConnection));
            Existing = existing ?? throw new ArgumentNullException(nameof(existing));
            SignlinkUid = signlinkUid;
        }

        public Player Transfer([NotNull] EntityPool<Player> players)
        {
            if (players == null) throw new ArgumentNullException(nameof(players));

            if (Existing.Connection.IsConnected()) return null;
            if (Existing.Connection.SignlinkId != SignlinkUid) return null;

            Existing.Connection.AssignNewSocket(NewConnection);
            Existing.Observatory.Clear();

            Existing.Server.Log.Debug(this, $"Reconnected client iid {Existing.UniqueEntityId}");

            return Existing;
        }
    }
}