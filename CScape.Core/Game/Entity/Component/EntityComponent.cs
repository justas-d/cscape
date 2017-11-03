using System;
using CScape.Core.Utility;
using CScape.Models;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public abstract class EntityComponent : IEntityComponent
    {
        public abstract int Priority { get; }
        public abstract void ReceiveMessage(IGameMessage msg);

        private LazyService<IMainLoop> _loop;
        private LazyService<ILogger> _log;

        [NotNull]
        public IEntity Parent { get; }

        [NotNull]
        public ILogger Log => _log.Value;

        [NotNull]
        public IMainLoop Loop => _loop.Value;

        protected EntityComponent([NotNull]IEntity parent)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _loop = new LazyService<IMainLoop>(parent.Server.Services);
            _log = new LazyService<ILogger>(parent.Server.Services);
        }
    }
}