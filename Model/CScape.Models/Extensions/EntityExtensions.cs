using System;
using System.Linq;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Models.Extensions
{
    public static class EntityExtensions
    {
        [CanBeNull]
        public static IVisionComponent GetVision(this IEntity ent) => ent.Components.Get<IVisionComponent>();
        [NotNull]
        public static IVisionComponent AssertGetVision(this IEntity ent) => ent.Components.AssertGet<IVisionComponent>();

        [CanBeNull]
        public static IClientPositionComponent GetClientPosition(this IEntity ent) => ent.Components.Get<IClientPositionComponent>();
        [NotNull]
        public static IClientPositionComponent AssertGetClientPosition(this IEntity ent) => ent.Components.AssertGet<IClientPositionComponent>();

        [CanBeNull]
        public static ICombatStatComponent GetCombatStats(this IEntity ent) => ent.Components.Get<ICombatStatComponent>();
        [NotNull]
        public static ICombatStatComponent AssertGetCombatStats(this IEntity ent) => ent.Components.AssertGet<ICombatStatComponent>();

        [CanBeNull]
        public static IGroundItemComponent GetGroundItem(this IEntity ent) => ent.Components.Get<IGroundItemComponent>();
        [NotNull]
        public static IGroundItemComponent AssertGetGroundItem(this IEntity ent) => ent.Components.AssertGet<IGroundItemComponent>();

        [CanBeNull]
        public static IHealthComponent GetHealth(this IEntity ent) => ent.Components.Get<IHealthComponent>();
        [NotNull]
        public static IHealthComponent AssertGetHealth(this IEntity ent) => ent.Components.AssertGet<IHealthComponent>();

        [CanBeNull]
        public static IInterfaceComponent GetInterfaces(this IEntity ent) => ent.Components.Get<IInterfaceComponent>();
        [NotNull]
        public static IInterfaceComponent AssertGetInterfaces(this IEntity ent) => ent.Components.AssertGet<IInterfaceComponent>();

        [CanBeNull]
        public static IInventoryComponent GetInventory(this IEntity ent) => ent.Components.Get<IInventoryComponent>();
        [NotNull]
        public static IInventoryComponent AssertGetInventory(this IEntity ent) => ent.Components.AssertGet<IInventoryComponent>();

        [CanBeNull]
        public static INpcComponent GetNpc(this IEntity ent) => ent.Components.Get<INpcComponent>();
        [NotNull]
        public static INpcComponent AssertGetNpc(this IEntity ent) => ent.Components.AssertGet<INpcComponent>();

        [CanBeNull]
        public static IPlayerComponent GetPlayer(this IEntity ent) => ent.Components.Get<IPlayerComponent>();
        [NotNull]
        public static IPlayerComponent AssertGetPlayer(this IEntity ent) => ent.Components.AssertGet<IPlayerComponent>();

        [CanBeNull]
        public static ISkillComponent GetSkills(this IEntity ent) => ent.Components.Get<ISkillComponent>();
        [NotNull]
        public static ISkillComponent AssertGetSkills(this IEntity ent) => ent.Components.AssertGet<ISkillComponent>();

        [NotNull]
        public static ITransform GetTransform(this IEntity ent) => ent.Components.AssertGet<ITransform>();

        [CanBeNull]
        public static IVisionResolver GetVisionResolver(this IEntity ent) => ent.Components.Get<IVisionResolver>();
        [NotNull]
        public static IVisionResolver AssertGetVisionResolver(this IEntity ent) => ent.Components.AssertGet<IVisionResolver>();

        /// <summary>
        /// Sends a message to each visible not dead entity this entity can see.
        /// </summary>
        public static void Broadcast(this IVisionComponent vision, IGameMessage msg)
        {
            foreach (var ent in vision.GetVisibleEntities().Select(e =>e.Get()))
                ent.SendMessage(msg);
        }

        public static bool IsDead(this IEntityHandle handle) => handle.System.IsDead(handle);
        public static IEntity Get(this IEntityHandle handle) => handle.System.Get(handle);
        public static bool Destroy(this IEntityHandle handle) => handle.System.Destroy(handle);

        /// <summary>
        /// Resolves vision between two entities, where the main entity (<see cref="main"/>) is the entity trying to see the other entity <see cref="oth"/>.'
        /// 1) Whatever any other circumstace may be, if <see cref="main"/> doesn't have a vision component OR does have one but cannot see <see cref="oth"/>, we regard that as both entities not seeing each other.
        /// 2) If <see cref="main"/> does have a vision component and can see <see cref="oth"/>, but oth does not have a vision component, then we leave it as that and return true.
        /// 3) If <see cref="main"/> does see <see cref="oth"/> but oth has a vision component and cannot see <see cref="main"/>, we regard that as both entities not being able to see each other.
        /// </summary>
        /// <param name="main">The main entity around which the sight evaluation will be centered upon.</param>
        /// <param name="oth">The target entity.</param>
        /// <returns>True if both entities can see each other, false otherwise.</returns>
        public static bool CanSee(this IEntity main, IEntity oth)
        {
            var mainVision = main.GetVision();
            var othVision = oth.GetVision();

            // 1
            if (mainVision == null) return false;

            // 3
            if (othVision != null)
                return mainVision.CanSee(oth) && othVision.CanSee(main);

            // 1 & 2
            return mainVision.CanSee(oth);
        }
    }
}
