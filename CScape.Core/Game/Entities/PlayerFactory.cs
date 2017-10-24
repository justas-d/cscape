using System;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Core.Database.Entity;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Network.Entity.Component;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Entity.Factory;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class PlayerFactory : IPlayerFactory
    {
        public const int InvalidPlayerId = -1;

        [NotNull]
        public IEntitySystem EntitySystem { get; }

        // username lookup
        private Dictionary<string, IEntityHandle> _usernameLookup;
        // instance id lookup
        private readonly IEntityHandle[] _players;

        public IReadOnlyList<IEntityHandle> All => _players;

        private ILogger Log => EntitySystem.Server.Services.ThrowOrGet<ILogger>();

        public PlayerFactory([NotNull] IEntitySystem entitySystem)
        {
            EntitySystem = entitySystem ?? throw new ArgumentNullException(nameof(entitySystem));

            // create an array of entity handles which will be all initialized to null
            // then let a list wrap around that array.
            _players = new IEntityHandle[entitySystem.Server.Services.ThrowOrGet<IGameServerConfig>().MaxPlayers];
        }

        public IEntityHandle Get(string username)
        {
            if (_usernameLookup.ContainsKey(username))
                return _usernameLookup[username];
            return null;
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

        public IEntityHandle Get(int id)
        {
            // check if id is in range.
            if (0 >= id && _players.Length > id)
            {
                return _players[id];
            }
            return null;
        }

        public IEntityHandle Create(IPlayerModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

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
            
            // todo : finish adding player components
            ent.Components.Add(new MovementActionComponent(ent));
            ent.Components.Add(new HealthComponent(ent, 10, model.Health));
            ent.Components.Add(new MessageLogComponent(ent));
            ent.Components.Add(new TileMovementComponent(ent));
            ent.Components.Add<IInventoryComponent>(new PlayerInventoryComponent(ent));
            ent.Components.Add(new InterfaceComponent(ent));
            ent.Components.Add(new PlayerComponent(
                ent, 
                model.Id,
                id,
                DestroyCallback));

            var check = ent.AreComponentRequirementsSatisfied(out var msg);
            if (!check)
                Debug.Fail(msg);
            
            // TODO : add skills to SkillComponent for players

            _players[id] = entHandle;
            _usernameLookup.Add(msg, entHandle);

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
