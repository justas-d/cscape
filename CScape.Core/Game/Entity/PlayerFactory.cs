using System;
using System.Collections.Generic;
using System.Diagnostics;
using CScape.Core.Database;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Item;
using CScape.Core.Game.Skill;
using CScape.Core.Network;
using CScape.Core.Network.Entity.Component;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Factory;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public sealed class PlayerFactory : IPlayerFactory
    {
        public const int InvalidPlayerId = -1;

        [NotNull]
        public IEntitySystem EntitySystem { get; }

        // username lookup
        private readonly Dictionary<string, IEntityHandle> _usernameLookup = new Dictionary<string, IEntityHandle>();
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

        public IEntityHandle Create(
            [NotNull] SerializablePlayerModel model, 
            [NotNull] SocketContext socket,
            [NotNull] IPacketParser packetParser,
            [NotNull] IPacketHandlerCatalogue packets)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (packetParser == null) throw new ArgumentNullException(nameof(packetParser));
            if (packets == null) throw new ArgumentNullException(nameof(packets));

            var id = GetPlayerId();
            if (id == InvalidPlayerId)
                return null;
            
            Debug.Assert(_players[id] == null);

            var entHandle = EntitySystem.Create($"Entity for player {model.Username}");
            var ent = entHandle.Get();

            ent.Components.Add(new MessageLogComponent(ent));
            ent.Components.Add(new MessageNetworkSyncComponent(ent));

            ent.Components.Add(new DebugStatNetworkSyncComponent(ent));

            ent.Components.Add(new NetworkingComponent(ent, socket, packetParser));
            ent.Components.Add(new PacketDispatcherComponent(ent, packets));
            ent.Components.Add(new FlagAccumulatorComponent(ent));

            ent.Components.Add(new ClientPositionComponent(ent));
            ent.Components.Add(new RegionNetworkSyncComponent(ent));

            var health = new HealthComponent(ent);
            ent.Components.Add(health);

            ent.Components.Add(new CombatStatComponent(ent));
            ent.Components.Add(new CombatStatNetworkSyncComponent(ent));

            ent.Components.Add(new InterfaceComponent(ent));
            ent.Components.Add(new InterfaceNetworkSyncComponent(ent));

            var skills = new SkillComponent(ent);
            ent.Components.Add(skills);
            ent.Components.Add(new SkillNetworkSyncComponent(ent));

            ent.Components.Add(new VisionComponent(ent));

            ent.Components.Add(new NpcNetworkSyncComponent(ent));
            ent.Components.Add(new PlayerNetworkSyncComponent(ent));
            ent.Components.Add(new GroundItemNetworkSyncComponent(ent));

            ent.Components.Add(new TileMovementComponent(ent));
            ent.Components.Add(new MovementActionComponent(ent));
            
            ent.Components.Add(new PlayerInventoryComponent(ent, 
                new ListItemContainer(ent, model.Backpack), 
                new PlayerEquipmentContainer(ent, model.Equipment), 
                new ListItemContainer(ent, model.Bank)));
            ent.Components.Add(new PlayerComponent(ent, 
                model.Username,
                model.Apperance,
                true,
                model.TitleId,
                id,
                DestroyCallback));

            var check = ent.AreComponentRequirementsSatisfied(out var msg);
            if (!check) throw new InvalidOperationException(msg);

            // setup skills
            foreach (var skill in model.Skils)
                skills.All.Add(skill.Key, new NormalSkillModel(skill.Key, skill.Value.Boost, skill.Value.Experience));

            // setup health
            var skillDb = EntitySystem.Server.Services.ThrowOrGet<SkillDb>();
            health.SetNewMaxHealth(skills.All[skillDb.Hitpoints].Level);
            health.SetNewHealth(model.Health);
          
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
