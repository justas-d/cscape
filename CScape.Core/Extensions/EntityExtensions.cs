using System;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Entity.Component;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Extensions
{
    public static class EntityExtensions
    {
        [CanBeNull]
        public static PlayerInventoryComponent GetPlayerContainers(this IEntity ent) => ent.Components.Get<PlayerInventoryComponent>();
        [NotNull]
        public static PlayerInventoryComponent AssertGetPlayerContainers(this IEntity ent) => ent.Components.AssertGet<PlayerInventoryComponent>();


        [CanBeNull]
        public static MovementActionComponent GetMovementAction(this IEntity ent) => ent.Components.Get<MovementActionComponent>();
        [NotNull]
        public static MovementActionComponent AssertGetMovementAction(this IEntity ent) => ent.Components.AssertGet<MovementActionComponent>();

        [CanBeNull]
        public static NetworkingComponent GetNetwork(this IEntity ent) => ent.Components.Get<NetworkingComponent>();
        [NotNull]
        public static NetworkingComponent AssertGetNetwork(this IEntity ent) => ent.Components.AssertGet<NetworkingComponent>();

        /// <summary>
        /// Sends a system message to the entity.
        /// </summary>
        public static void SystemMessage(this IEntity ent, [NotNull] string msg, SystemMessageFlags flags = SystemMessageFlags.None)
        {
            if (string.IsNullOrEmpty(msg)) return;

            ent.SendMessage(new SystemMessage(msg, flags));
        }

        public static void ShowParticleEffect(this IEntity ent, [NotNull] ParticleEffect eff)
        {
            if (eff == null) throw new ArgumentNullException(nameof(eff));
            ent.SendMessage(new ParticleEffectMessage(eff));
        }
    }
}
