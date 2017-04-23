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

        public void Transfer(MainLoop loop)
        {
            var player = new Player(this);

            if (!string.IsNullOrEmpty(Server.Config.Greeting))
                player.SendSystemChatMessage(Server.Config.Greeting);

            loop.Player.Enqueue(player);
        }
    }
}