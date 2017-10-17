using System;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Core.Database.Entity;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Injection;
using CScape.Core.Network.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class PlayerFactory : IPlayerFactory
    {
        public const int InvalidPlayerId = -1;

        [NotNull]
        public IEntitySystem EntitySystem { get; }

        private readonly EntityHandle[] _players;

        [NotNull]
        public IReadOnlyList<EntityHandle> All => _players;

        private ILogger Log => EntitySystem.Server.Services.ThrowOrGet<ILogger>();

        public PlayerFactory([NotNull] IEntitySystem entitySystem)
        {
            EntitySystem = entitySystem ?? throw new ArgumentNullException(nameof(entitySystem));

            // create an array of entity handles which will be all initialized to null
            // then let a list wrap around that array.
            _players = new EntityHandle[entitySystem.Server.Services.ThrowOrGet<IGameServerConfig>().MaxPlayers]);
        }

        /// <summary>
        /// Finds and returns the next free player id.
        /// </summary>
        private int GetPlayerId()
        {
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] == null)
                    return i;
            }

            return InvalidPlayerId;
        }

        public EntityHandle Get(int id)
        {
            // check if id is in range.
            if (0 >= id && _players.Length > id)
            {
                return _players[id];
            }
            return null;
        }

        public EntityHandle Create(IPlayerModel model, ISocketContext ctx)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            var id = GetPlayerId();
            if (id == InvalidPlayerId)
                return null;
            
            Debug.Assert(_players[id] == null);

            var entHandle = EntitySystem.Create($"Entity for player {model.Id}");
            var ent = entHandle.Get();
            
            ent.Components.Add(new ClientPositionComponent(ent));

            // TODO : apply hitpoints skill to HealthComponent when constructing player
            
            ent.Components.Add(new DbPlayerSyncComponent(ent));

            ent.Components.Add(new PacketDispatcherComponent(ent,));
            ent.Components.Add(new NetworkingComponent(ent, ctx,));
            ent.Components.Add(new MessageNetworkSyncComponent(ent));
            ent.Components.Add(new RegionNetworkSyncComponent(ent));
            
            ent.Components.Add(new MovementActionComponent(ent));
            ent.Components.Add(new HealthComponent(ent, 10, model.Health));
            ent.Components.Add(new MessageLogComponent(ent));
            ent.Components.Add(new TileMovementComponent(ent));
            ent.Components.Add(new PlayerComponent(
                ent, 
                model.Id,
                id,
                DestroyCallback));

            ent.AssertComponentRequirementsSatisfied();

            _players[id] = entHandle;

            return entHandle;
        }

        private void DestroyCallback([NotNull] PlayerComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            Log.Normal(this, $"Freeing player slot {component.PlayerId} {component.Username}");
            _players[component.PlayerId] = null;
        }
    }
}
