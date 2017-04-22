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
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            SignlinkUid = signlinkUid;
        }

        public Player Transfer([NotNull] EntityPool<Player> players)
        {
            if (players == null) throw new ArgumentNullException(nameof(players));

            var player = new Player(this);
            players.Add(player);

            return player;
        }
    }
}