using System;
using System.Collections.Generic;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class PlayerFactory : IPlayerFactory
    {
        public const int InvalidPlayerId = -1;

        [NotNull]
        public IEntitySystem EntitySystem { get; }

        private readonly List<EntityHandle> _players;

        [NotNull]
        public IReadOnlyList<EntityHandle> Players => _players;

        public PlayerFactory([NotNull] IEntitySystem entitySystem)
        {
            EntitySystem = entitySystem ?? throw new ArgumentNullException(nameof(entitySystem));

            _players = new List<EntityHandle>(entitySystem.Server.Services
                .ThrowOrGet<IGameServerConfig>().MaxPlayers);
        }

        /// <summary>
        /// Finds and returns the next free player id.
        /// </summary>
        private int GetPlayerId()
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i] == null)
                    return i;
            }

            return InvalidPlayerId;
        }
       
        public EntityHandle Create(IPlayerModel model, ISocketContext ctx)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            var id = GetPlayerId();
            if (id == InvalidPlayerId)
                return null;
            

            var entHandle = EntitySystem.Create($"Entity for player {model.Id}");
            var ent = entHandle.Get();
            
            ent.Components.Add(new ClientPositionComponent(ent));

            // TODO : apply hitpoints skill to HealthComponent when constructing player
            ent.Components.Add(new HealthComponent(ent, 10, model.Health)); 
            ent.Components.Add(new DbPlayerSyncComponent(ent));
            ent.Components.Add(new NetPlayerSyncComponent(ent, ctx));
            ent.Components.Add(new TileMovementComponent(ent));
            ent.Components.Add(new PlayerComponent(ent, model.Id, id));

            ent.AssertComponentRequirementsSatisfied();

            _players.Add(entHandle);

            return entHandle;
        }
    }
}
