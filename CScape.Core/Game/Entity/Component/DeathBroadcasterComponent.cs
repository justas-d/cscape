using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    [RequiresComponent(typeof(IVisionComponent))]
    public sealed class DeathBroadcasterComponent : EntityComponent
    {
        public override int Priority => (int)ComponentPriority.Invariant;

        public DeathBroadcasterComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        private void NotifyNearbyEntitiesOfDeath()
            => Parent.AssertGetVision().Broadcast(EntityMessage.NearbyEntityQueuedForDeath(Parent.Handle));

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.QueuedForDeath:
                {
                    NotifyNearbyEntitiesOfDeath();
                    break;
                }
            }
        }
    }
}