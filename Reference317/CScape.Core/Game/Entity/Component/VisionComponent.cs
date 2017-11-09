using System.Collections.Generic;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class VisionComponent : EntityComponent, IVisionComponent
    {
        public override int Priority => (int)ComponentPriority.Invariant;
        
        /// <summary>
        /// "Can see up to n tiles".
        /// </summary>
        public int ViewRange { get; set; } = EntityVision.DefaultViewRange;
        
        public VisionComponent(IEntity parent)
            :base(parent)
        {
            
        }

        public bool CanSee(IEntity ent) 
            => EntityVision.CanSee(Parent, ent, ViewRange);

        public IEnumerable<IEntityHandle> GetVisibleEntities() 
            => EntityVision.GetVisibleEntities(Parent, ViewRange);

        public override void ReceiveMessage(IGameMessage msg)
        {
            // ignored
        }
    }
}