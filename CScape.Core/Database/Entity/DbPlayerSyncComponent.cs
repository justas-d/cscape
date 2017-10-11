using System;
using CScape.Core.Game.Entities;

namespace CScape.Core.Database.Entity
{
    /// <summary>
    /// Responsible for syncing the player to the database.
    /// </summary>
    public sealed class DbPlayerSyncComponent : EntityComponent
    {
        public override int Priority { get; }
        
        public DbPlayerSyncComponent(Game.Entities.Entity parent)
            :base(parent)
        {
            
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}