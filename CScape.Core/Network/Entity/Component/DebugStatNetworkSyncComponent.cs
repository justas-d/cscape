using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Packet;
using CScape.Models;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    // TODO : DebugStatNetworkSyncComponent isn't doing anything visible to the client
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class DebugStatNetworkSyncComponent : EntityComponent
    {
        public override int Priority => (int)ComponentPriority.DebugStatSync;

        public DebugStatNetworkSyncComponent([NotNull]IEntity parent) : base(parent)
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
            if (msg.EventId == (int)MessageId.NetworkPrepare)
                Sync();
        }
    }
}