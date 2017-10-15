using System;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Items;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class ItemFactory : IItemFactory
    {
        public IEntitySystem System { get; }

        public ItemFactory([NotNull] IEntitySystem system)
        {
            System = system ?? throw new ArgumentNullException(nameof(system));
        }

        public EntityHandle CreatePlayerDrop(ItemStack stack, PlayerComponent player, string name)
        {
            var handle = System.Create($"Player dropped item: {name} ({stack.Id.Name}: {stack.Amount}");
            var ent = handle.Get();

            var item = new PlayerDroppedItemComponent(ent, stack, null, player.Username);
            ent.Components.Add(item);
            ent.Components.Add((IVisionResolver)item);

            return handle;
        }

        public EntityHandle Create(ItemStack stack, string name)
        {
            var handle = System.Create($"Ground item: {name} ({stack.Id.Name}: {stack.Amount}");
            var ent = handle.Get();

            ent.Components.Add(new GroundItemComponent(ent, stack, null));

            return handle;
        }
    }
}