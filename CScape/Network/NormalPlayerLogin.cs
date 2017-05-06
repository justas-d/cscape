using System;
using System.Net.Sockets;
using CScape.Game.Entity;
using CScape.Game.Model;
using JetBrains.Annotations;

namespace CScape.Network
{
    public class NormalPlayerLogin : IPlayerLogin
    {
        public GameServer Server { get; }
        public PlayerModel Model { get; }
        public Socket Connection { get; }
        public int SignlinkUid { get; }

        public NormalPlayerLogin([NotNull] GameServer server, [NotNull] PlayerModel model,
            [NotNull] Socket connection, int signlinkUid)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Model = model ?? throw new ArgumentNullException(nameof(model));
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