using System;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    /// <summary>
    /// Responsible for keeping the player in sync.
    /// </summary>
    public sealed class NetPlayerSyncComponent : IEntityComponent
    {
        [NotNull]
        public ISocketContext Socket { get; }
        public Entity Parent { get; }

        public NetPlayerSyncComponent(
            [NotNull] Entity parent, [NotNull] ISocketContext socket)
        {
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public void Update(IMainLoop loop)
        {

        }

        public void ReceiveMessage(EntityMessage msg)
        {
            throw new NotImplementedException();
        }
    }

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

    public sealed class PlayerComponent : IEntityComponent
    {
        public Entity Parent { get; }

        public PlayerComponent([NotNull] Entity parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public void Update(IMainLoop loop)
        {

        }

        public void ReceiveMessage(EntityMessage msg)
        {

        }
    }
}
