using System;
using CScape.Models;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public abstract class EntityComponent : IEntityComponent
    {
        public abstract int Priority { get; }
        public abstract void ReceiveMessage(IGameMessage msg);

        [NotNull]
        public IEntity Parent { get; }

        [NotNull]
        public ILogger Log { get; }

        [NotNull]
        public IMainLoop Loop { get; }

        protected EntityComponent([NotNull]IEntity parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            Log = parent.Server.Services.ThrowOrGet<ILogger>();
            Loop = parent.Server.Services.ThrowOrGet<IMainLoop>();
        }
    }
}