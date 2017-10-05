using System;
using System.Collections.Generic;
using CScape.Core.Game.Entities.Fragment.Component;
using CScape.Core.Game.Entities.Fragment.Network;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class PlayerFactory : IPlayerFactory
    {
        public const int InvalidPlayerId = -1;

        [NotNull]
        public IEntitySystem EntitySystem { get; }

        private readonly List<EntityHandle> _players;

        [NotNull]
        public IReadOnlyList<EntityHandle> Players => _players;

        private ILogger Log => EntitySystem.Server.Services.ThrowOrGet<ILogger>();

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
            
            ent.Components.Add(new DbPlayerSyncComponent(ent));

            ent.Components.Add(new PacketDispatcherComponent(ent,));
            ent.Components.Add(new NetworkingComponent(ent, ctx,));
            ent.Network.Add(new MessageSyncNetFragment(ent));
            ent.Network.Add(new RegionSyncNetFragment(ent));
            
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

            _players.Add(entHandle);

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
