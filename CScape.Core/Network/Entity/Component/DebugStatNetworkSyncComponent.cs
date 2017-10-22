using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Network.Packet;
using CScape.Models;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class DebugStatNetworkSyncComponent : EntityComponent
    {
        public override int Priority { get; }

        public DebugStatNetworkSyncComponent([NotNull] Game.Entities.Entity parent) : base(parent)
        {
        }        

        private void Sync()
        {
            var net = Parent.Components.AssertGet<NetworkingComponent>();
            var loop = Parent.Server.Services.ThrowOrGet<IMainLoop>();

            net.SendPacket(new DebugInfoPacket((int)loop.GetDeltaTime(), (int)loop.TickProcessTime));
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            if (msg.EventId == (int)MessageId.NetworkUpdate)
                Sync();
        }
    }
}