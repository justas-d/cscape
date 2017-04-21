using System;
using System.Linq;
using System.Net.Sockets;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network
{
    public class ReconnectPlayerLogin : IPlayerLogin
    {
        public string Username { get; }
        public Socket NewConnection { get; }
        public int SignlinkUid { get; }

        public ReconnectPlayerLogin([NotNull] string username, [NotNull] Socket newConnection, int signlinkUid)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            NewConnection = newConnection ?? throw new ArgumentNullException(nameof(newConnection));
            SignlinkUid = signlinkUid;
        }

        public void Transfer([NotNull] EntityPool<Player> players)
        {
            if (players == null) throw new ArgumentNullException(nameof(players));

            var player = players.FirstOrDefault(p => p.Username == Username);

            if (player == null) return;
            if (player.Connection.IsConnected()) return;
            if (player.Connection.SignlinkId != SignlinkUid) return;

            player.Connection.AssignNewSocket(NewConnection);
            player.Server.Log.Debug(this, $"Reconnected client iid {player.InstanceId}");
        }
    }
}