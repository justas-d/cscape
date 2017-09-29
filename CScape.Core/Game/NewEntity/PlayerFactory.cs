using System;
using System.Collections.Generic;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class PlayerFactory
    {
        public const int InvalidPlayerId = -1;

        [NotNull]
        public IEntitySystem EntitySystem { get; }

        private List<PlayerComponent> _players;
        public IReadOnlyList<PlayerComponent> Players => _players;

        public PlayerFactory([NotNull] IEntitySystem entitySystem)
        {
            EntitySystem = entitySystem ?? throw new ArgumentNullException(nameof(entitySystem));

            _players = new List<PlayerComponent>(entitySystem.Server.Services
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

        /// <summary>
        /// Creates a player entity.
        /// </summary>
        /// <param name="model">The player model which the new player entity will represent db sync with.</param>
        /// <param name="ctx">The connection context with which the player will net sync with</param>
        /// <returns>An <see cref="EntityHandle"/> pointing to the new player entity or null if the player list is full.</returns>
        /// <exception cref="EntityComponentNotSatisfied">One of the components of the entity is not satisfied</exception>
        public EntityHandle Create([NotNull] IPlayerModel model, [NotNull] ISocketContext ctx)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            var id = GetPlayerId();
            if (id == InvalidPlayerId)
                return null;
            

            var entHandle = EntitySystem.Create($"Entity for player {model.Id}");
            var ent = entHandle.Get();
            
            ent.AddComponent(new ClientPositionComponent(ent));

            // TODO : apply hitpoints skill to HealthComponent when constructing player
            ent.AddComponent(new HealthComponent(ent, 10, model.Health)); 
            ent.AddComponent(new DbPlayerSyncComponent(ent));
            ent.AddComponent(new NetPlayerSyncComponent(ent, ctx));
            ent.AddComponent(new PlayerComponent(ent, model.Id, id));

            ent.AssertComponentRequirementsSatisfied();

            return entHandle;
        }
    }
}
