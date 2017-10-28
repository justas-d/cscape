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
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Entity.Factory;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public sealed class PlayerFactory : InstanceFactory, IPlayerFactory
    {
        public const int InvalidPlayerId = -1;

        [NotNull]
        public IEntitySystem EntitySystem { get; }

        // username lookup
        private readonly Dictionary<string, IEntityHandle> _usernameLookup = new Dictionary<string, IEntityHandle>();

        public IReadOnlyList<IEntityHandle> All => InstanceLookup;

        private ILogger Log { get; }

        public PlayerFactory(IServiceProvider services) : base(services.ThrowOrGet<IGameServerConfig>().MaxPlayers)
        {
            EntitySystem = services.ThrowOrGet<IEntitySystem>();
            Log = services.ThrowOrGet<ILogger>();
        }

        public IEntityHandle Get(int id) => GetById(id);
        
        public IEntityHandle Get(string username)
        {
            if (_usernameLookup.ContainsKey(username))
                return _usernameLookup[username];
            return null;
        }



        // TODO : return value PlayerFactory.Create null checks
        [CanBeNull]
        public IEntityHandle Create(
            [NotNull] SerializablePlayerModel model, 
            [NotNull] ISocketContext socket,
            [NotNull] IPacketParser packetParser,
            [NotNull] IPacketHandlerCatalogue packets)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (socket == null) throw new ArgumentNullException(nameof(socket));
            if (packetParser == null) throw new ArgumentNullException(nameof(packetParser));
            if (packets == null) throw new ArgumentNullException(nameof(packets));

            var id = GetId();
            if (id == InvalidPlayerId)
                return null;
            
            Debug.Assert(InstanceLookup[id] == null);

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
            ent.Components.Add<IHealthComponent>(health);

            var cmb = new CombatStatComponent(ent);
            ent.Components.Add(cmb);
            ent.Components.Add<ICombatStatComponent>(cmb);
            ent.Components.Add(new CombatStatNetworkSyncComponent(ent));

            var interf = new InterfaceComponent(ent);
            ent.Components.Add(interf);
            ent.Components.Add<IInterfaceComponent>(interf);
            ent.Components.Add(new InterfaceNetworkSyncComponent(ent));

            var skills = new SkillComponent(ent);
            ent.Components.Add(skills);
            ent.Components.Add<ISkillComponent>(skills);
            ent.Components.Add(new SkillNetworkSyncComponent(ent));

            var vision = new VisionComponent(ent);
            ent.Components.Add(vision);
            ent.Components.Add<IVisionComponent>(vision);

            ent.Components.Add(new NpcNetworkSyncComponent(ent));
            ent.Components.Add(new PlayerNetworkSyncComponent(ent));
            ent.Components.Add(new GroundItemNetworkSyncComponent(ent));

            ent.Components.Add(new TileMovementComponent(ent));
            ent.Components.Add(new MovementActionComponent(ent));

            ent.Components.Add(new ItemActionDispatchComponent(ent));

            var inv = new PlayerInventoryComponent(ent,
                new ListItemContainer(ent, model.Backpack),
                new PlayerEquipmentContainer(ent, model.Equipment),
                new ListItemContainer(ent, model.Bank));

            ent.Components.Add(inv);
            ent.Components.Add<IInventoryComponent>(inv);

            var player = new PlayerComponent(ent, 
                model.Username,
                model.Apperance,
                true,
                model.TitleId,
                id,
                DestroyCallback);

            ent.Components.Add(player);
            ent.Components.Add<IPlayerComponent>(player);

            var check = ent.AreComponentRequirementsSatisfied(out var msg);
            if (!check) throw new InvalidOperationException(msg);

            // setup skills
            foreach (var skill in model.Skils)
                skills.All.Add(skill.Key, new NormalSkillModel(skill.Key, skill.Value.Boost, skill.Value.Experience));

            // setup health
            var skillDb = EntitySystem.Server.Services.ThrowOrGet<SkillDb>();
            health.SetNewMaxHealth(skills.All[skillDb.Hitpoints].Level);
            health.SetNewHealth(model.Health);
          
            InstanceLookup[id] = entHandle;
            _usernameLookup.Add(model.Username, entHandle);

            return entHandle;
        }

        private void DestroyCallback([NotNull] PlayerComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            Log.Normal(this, $"Freeing player slot {component.PlayerId} {component.Username}");

            InstanceLookup[component.PlayerId] = null;
            _usernameLookup.Remove(component.Username);
        }
    }
}
