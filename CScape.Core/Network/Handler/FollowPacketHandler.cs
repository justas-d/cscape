using System;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Factory;

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