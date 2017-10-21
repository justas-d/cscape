using System;
using System.Net.Sockets;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network;
using CScape.Models;
using JetBrains.Annotations;

namespace CScape.Core
{
    public class NormalPlayerLogin : IPlayerLogin
    {
        public IPlayerModel Model { get; }
        public Socket Connection { get; }
        public IServiceProvider Service { get; }
        public int SignlinkUid { get; }
        public bool IsHighDetail { get; }

        public NormalPlayerLogin(IServiceProvider service, [NotNull] IPlayerModel model,
            [NotNull] Socket connection, int signlinkUid, bool isHighDetail)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Service = service;
            SignlinkUid = signlinkUid;
            IsHighDetail = isHighDetail;
        }

        public void Transfer(IMainLoop loop)
        {
            var socket = new SocketContext(Service, Connection, SignlinkUid);
            var player = new Player(Model, socket, Service, IsHighDetail);

            var greet = Service.ThrowOrGet<IGameServerConfig>().Greeting;
            if (!string.IsNullOrEmpty(greet))
                player.SendSystemChatMessage(greet);
        }
    }
}