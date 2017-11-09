using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    /// <summary>
    /// Does nothing except signal the fact that the parent entity is marked for death.
    /// </summary>
    public sealed class MarkedForDeathComponent : EntityComponent
    {
        public override int Priority => (int) ComponentPriority.Invariant;

        public MarkedForDeathComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        
        public override void ReceiveMessage(IGameMessage msg)
        {
            
        }
    }
}
