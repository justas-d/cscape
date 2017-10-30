using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity.Component;
using CScape.Models;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Entity.Factory;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    public sealed class NpcFactory : InstanceFactory, INpcFactory
    {
        public IEntitySystem Entities { get; }

        public IEnumerable<IEntityHandle> All => InstanceLookup.Where(p => p != null);

        private ILogger _log;

        public NpcFactory(IServiceProvider services) : base(services.ThrowOrGet<IGameServerConfig>().MaxNpcs)
        {
            Entities = services.ThrowOrGet<IEntitySystem>();
            _log = services.ThrowOrGet<ILogger>();
        }

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

            return handle;
        }

        private void DestroyCallback(NpcComponent npc)
        {
            _log.Normal(this, $"Freeing npc slot {npc.NpcId} named {npc.Parent.Name}");
            InstanceLookup[npc.NpcId] = null;
        }

        public IEntityHandle Get(int id) => GetById(id);
    }
}