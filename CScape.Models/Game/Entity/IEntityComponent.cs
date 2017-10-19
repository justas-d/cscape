using System;

namespace CScape.Models.Game.Entity
{
    /// <summary>
    /// Updates run during the entity update pass
    /// </summary>
    public interface IEntityComponent
    {
        [NotNull]
        Entity Parent { get; }
        int Priority { get; }

        void ReceiveMessage([NotNull] GameMessage msg);
    }

    public abstract class EntityComponent : IEntityComponent
    {
        public abstract int Priority { get; }

        [NotNull]
        public Entity Parent { get; }

        [NotNull]
        public ILogger Log { get; }

        [NotNull]
        public IMainLoop Loop { get; }

        protected EntityComponent([NotNull ]Entity parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Log = parent.Server.Services.ThrowOrGet<ILogger>();
            Loop = parent.Server.Services.ThrowOrGet<IMainLoop>();
        }

        public abstract void ReceiveMessage(GameMessage msg);
    }
}