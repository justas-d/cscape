using System;
using System.Net.Sockets;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network
{
    public class NormalPlayerLogin : IPlayerLogin
    {
        public GameServer Server { get; }
        public IPlayerSaveData Data { get; }
        public Socket Connection { get; }
        public int SignlinkUid { get; }

        public NormalPlayerLogin([NotNull] GameServer server, [NotNull] IPlayerSaveData data, [NotNull] Socket connection, int signlinkUid)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            Server = server;
            Data = data;
            Connection = connection;
            SignlinkUid = signlinkUid;
        }

        public void Transfer([NotNull] EntityPool<Player> players)
        {
            if (players == null) throw new ArgumentNullException(nameof(players));
            players.Add(new Player(this));
        }
    }
}