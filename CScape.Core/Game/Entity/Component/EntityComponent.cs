using System;
using CScape.Core.Extensions;
using CScape.Models;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public abstract class EntityComponent : IEntityComponent
    {
        public abstract int Priority { get; }
        public abstract void ReceiveMessage(IGameMessage msg);

        private readonly Lazy<IMainLoop> _loop;
        private readonly Lazy<ILogger> _log;

        [NotNull]
        public IEntity Parent { get; }

        [NotNull]
        public ILogger Log => _log.Value;

        [NotNull]
        public IMainLoop Loop => _loop.Value;

        protected EntityComponent([NotNull]IEntity parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _loop = parent.Server.Services.GetLazy<IMainLoop>();
            _log = parent.Server.Services.GetLazy<ILogger>();
        }
    }
}