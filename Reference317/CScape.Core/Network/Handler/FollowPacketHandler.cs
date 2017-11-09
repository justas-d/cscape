using System;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;

namespace CScape.Core.Network.Handler
{
    public sealed class FollowPacketHandler : IPacketHandler
    {
        public byte[] Handles { get; } = { 128, 139 };

        private IPlayerCatalogue _players;

        public FollowPacketHandler(IServiceProvider services)
        {
            _players = services.ThrowOrGet<IPlayerCatalogue>();
        }

        public void Handle(IEntity entity, PacketMessage packet)
        {
            var id = packet.Data.ReadInt16() - 1;

            // find player
            var target = _players.Get(id);
            if (target == null)
                return;

            if (target.IsDead())
                return;

            if (target.Equals(entity.Handle))
                return;

            // target handle is good, make dir provider
            entity.SendMessage(EntityMessage.PlayerFollowTarget(target));
        }
    }
}