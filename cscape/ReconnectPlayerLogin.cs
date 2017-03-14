using System;
using System.Linq;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace cscape
{
    public class ReconnectPlayerLogin : IPlayerLogin
    {
        public string Username { get; }
        public Socket NewConnection { get; }
        public int SignlinkUid { get; }

        public ReconnectPlayerLogin([NotNull] string username, [NotNull] Socket newConnection, int signlinkUid)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (newConnection == null) throw new ArgumentNullException(nameof(newConnection));

            Username = username;
            NewConnection = newConnection;
            SignlinkUid = signlinkUid;
        }

        public void Transfer([NotNull] EntityPool<Player> players)
        {
            if (players == null) throw new ArgumentNullException(nameof(players));

            var player = players.FirstOrDefault(p => p.Username == Username);

            if (player == null) return;
            if (player.Connection.IsConnected()) return;
            if (player.SignlinkId != SignlinkUid) return;

            player.Connection = new Player.SocketContext(NewConnection);
            player.Server.Log.Debug(this, $"Reconnected client iid {player.InstanceId}");
        }
    }
}