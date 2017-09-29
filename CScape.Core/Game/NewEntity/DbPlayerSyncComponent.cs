using System;
using CScape.Core.Injection;

namespace CScape.Core.Game.NewEntity
{
    /// <summary>
    /// Responsible for syncing the player to the database.
    /// </summary>
    public sealed class DbPlayerSyncComponent : IEntityComponent
    {
        public Entity Parent { get; }

        public DbPlayerSyncComponent(Entity parent)
        {
            Parent = parent;
        }

        public void Update(IMainLoop loop)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(EntityMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}