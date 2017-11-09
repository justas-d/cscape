using System;
using System.Net.Sockets;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Factory;
using CScape.Core.Json;
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

        public PlayerCatalogue Catalogue { get; }
        public IServiceProvider Services { get; }
        public string Greeting { get; }
        public int SignlinkUid { get; }

        public NormalPlayerLogin(
            IServiceProvider services,
            string greeting,
            [NotNull] SerializablePlayerModel model,
            [NotNull] Socket connection, 
            int signlinkUid)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Catalogue = services.ThrowOrGet<PlayerCatalogue>();
            Services = services;
            Greeting = greeting;
            SignlinkUid = signlinkUid;
        }

        public void Transfer(IMainLoop loop)
        {
            var socket = new SocketContext(Services, Connection, SignlinkUid);
            var player = Catalogue.Create(
                Model,
                socket,
                loop.Server.Services.ThrowOrGet<IPacketParser>(),
                loop.Server.Services.ThrowOrGet<IPacketHandlerCatalogue>());
            
            if (!string.IsNullOrEmpty(Greeting))
                player.Get().SystemMessage(Greeting);
        }
    }
}