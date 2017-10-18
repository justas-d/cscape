using System;
using CScape.Core.Data;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Directions;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;

namespace CScape.Core.Network.Handler
{
    public sealed class FollowPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = { 128, 139 };

        private IPlayerFactory _players;

        public FollowPacketHandler(IServiceProvider services)
        {
            _players = services.ThrowOrGet<IPlayerFactory>();
        }

        public void Handle(Game.Entities.Entity entity, PacketMetadata packet)
        {
            var id = packet.Data.ReadInt16();

            // find player
            var target = _players.Get(id);
            if (target == null)
                return;

            if (target.IsDead())
                return;

            var player = target.Get().Components.Get<PlayerComponent>();

            // target handle is good, make dir provider
            entity.SendMessage(
                new GameMessage(
                    null, GameMessage.Type.NewPlayerFollowTarget, player));
        }
    }
}