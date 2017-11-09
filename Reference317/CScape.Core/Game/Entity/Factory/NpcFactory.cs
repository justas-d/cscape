using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Models;
using CScape.Models.Data;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;

namespace CScape.Core.Game.Entity.Factory
{
    public sealed class NpcFactory : InstanceFactory, INpcFactory
    {
        private Lazy<IEntitySystem> _system;
        private Lazy<ILogger> _log;

        public IEntitySystem Entities => _system.Value;
        public ILogger Log => _log.Value;

        public IEnumerable<IEntityHandle> All => InstanceLookup.Where(p => p != null);
        
        public NpcFactory(IServiceProvider services) 
            : base(services.ThrowOrGet<IConfigurationService>().GetInt(ConfigKey.MaxNpcs))
        {
            _log = services.GetLazy<ILogger>();
            _system = services.GetLazy<IEntitySystem>();
        }

        // TODO : replace definition id with an INpcDefinition interface?
        public IEntityHandle Create(string name, int definitionId)
        {
            var handle = Entities.Create($"Npc: {name}");
            var ent = handle.Get();

            var id = GetId();
            if (id == InvalidId)
                return null;

            var vision = new VisionComponent(ent);
            ent.Components.Add(vision);
            ent.Components.Add<IVisionComponent>(vision);

            ent.Components.Add(new MovementActionComponent(ent));
            ent.Components.Add(new TileMovementComponent(ent));
            ent.Components.Add(new FlagAccumulatorComponent(ent));
            ent.Components.Add(new MarkedForDeathBroadcasterComponent(ent));

            // TODO : set npc health according to it's definition when creating the npc.
            var health = new HealthComponent(ent);
            ent.Components.Add(health);
            ent.Components.Add<IHealthComponent>(health);

            var npc = new NpcComponent(ent, (short) definitionId.Clamp(0, short.MaxValue),
                (short) id.Clamp(0, short.MaxValue), DestroyCallback);
            ent.Components.Add(npc);
            ent.Components.Add<INpcComponent>(npc);


            Debug.Assert(InstanceLookup[id] == null);

            InstanceLookup[id] = handle;

            ent.SendMessage(NotificationMessage.Initialize);

            return handle;
        }

        private void DestroyCallback(NpcComponent npc)
        {
            Log.Normal(this, $"Freeing npc slot {npc.InstanceId} named {npc.Parent.Name}");
            InstanceLookup[npc.InstanceId] = null;
        }

        public IEntityHandle Get(int id) => GetById(id);
    }
}