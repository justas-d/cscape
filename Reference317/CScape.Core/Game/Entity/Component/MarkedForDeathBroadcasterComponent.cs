using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    [RequiresComponent(typeof(IVisionComponent))]
    public sealed class MarkedForDeathBroadcasterComponent : EntityComponent
    {
        public override int Priority => (int)ComponentPriority.Invariant;

        public MarkedForDeathBroadcasterComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        private void AddMarkedForDeathComponent()
        {
            Parent.Components.Add(new MarkedForDeathComponent(Parent));
        }

        private void NotifyNearbyEntitiesOfDeath()
        {
            AddMarkedForDeathComponent();
            Parent.AssertGetVision().Broadcast(EntityMessage.NearbyEntityQueuedForDeath(Parent.Handle));
        }

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