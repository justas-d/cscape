using System;
using System.Net.Sockets;
using CScape.Core.Database;
using CScape.Core.Game.Entity;
using CScape.Models;
using CScape.Models.Extensions;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public class NormalPlayerLogin : IPlayerLogin
    {
        [NotNull]
        public SerializablePlayerModel Model { get; }
        [NotNull]
        public Socket Connection { get; }

        [NotNull]
        public IServiceProvider Services { get; }
        public int SignlinkUid { get; }

        private readonly string _greeting;
        private readonly PlayerFactory _factory;

        public NormalPlayerLogin(
            IServiceProvider services,
            [NotNull] SerializablePlayerModel model,
            [NotNull] Socket connection, 
            int signlinkUid)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Services = services;
            SignlinkUid = signlinkUid;

            _greeting = services.ThrowOrGet<IGameServerConfig>().Greeting;
            _factory = services.ThrowOrGet<PlayerFactory>();
        }

        public void Transfer(IMainLoop loop)
        {
            var socket = new SocketContext(Services, Connection, SignlinkUid);
            var player = _factory.Create(
                Model,
                socket,
                Services.ThrowOrGet<PacketParser>(),
                Services.ThrowOrGet<IPacketHandlerCatalogue>());
            
            if (!string.IsNullOrEmpty(_greeting))
                player.Get().SystemMessage(_greeting);
        }
    }
}