using System;
using CScape.Core.Game.Entities.Component;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;

namespace CScape.Core.Database.Entity
{
    /// <summary>
    /// Responsible for syncing the player to the database.
    /// </summary>
    public sealed class DbPlayerSyncComponent : EntityComponent
    {
        public override int Priority { get; }
        
        public DbPlayerSyncComponent(IEntity parent)
            :base(parent)
        {
            
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}