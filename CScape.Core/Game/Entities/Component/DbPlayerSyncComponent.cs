using System;
using CScape.Core.Game.Entities.Interface;

namespace CScape.Core.Game.Entities.Component
{
    /// <summary>
    /// Responsible for syncing the player to the database.
    /// </summary>
    public sealed class DbPlayerSyncComponent : EntityComponent
    {
        // TODO : DbPlayerSyncComponent
        public override int Priority { get; }
        
        public DbPlayerSyncComponent(Entity parent)
            :base(parent)
        {
            
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}