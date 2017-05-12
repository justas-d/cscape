using System;
using System.Net.Sockets;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Core
{
    public class NormalPlayerLogin : IPlayerLogin
    {
        public IPlayerModel Model { get; }
        public Socket Connection { get; }
        public IServiceProvider Service { get; }
        public int SignlinkUid { get; }

        public NormalPlayerLogin(IServiceProvider service, [NotNull] IPlayerModel model,
            [NotNull] Socket connection, int signlinkUid)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Service = service;
            SignlinkUid = signlinkUid;
        }

        public void Transfer(IMainLoop loop)
        {
            var player = new Player(this);

            var greet = Service.ThrowOrGet<IGameServerConfig>().Greeting;
            if (!string.IsNullOrEmpty(greet))
                player.SendSystemChatMessage(greet);

            loop.Player.Enqueue(player);
        }
    }
}