using System.Diagnostics;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.InteractingEntity;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class EntityMessage
    {
        private readonly object _data;

        [CanBeNull]
        public IEntityComponent Sender { get; }
        public EventType Event { get; }

        public static EntityMessage FrameEnd { get; } = new EntityMessage(null, EventType.FrameEnd, null);
        public static EntityMessage FrameUpdate { get; } = new EntityMessage(null, EventType.FrameUpdate, null);
        public static EntityMessage NetworkUpdate { get; } = new EntityMessage(null, EventType.NetworkUpdate, null);
        public static EntityMessage DatabaseUpdate { get; } = new EntityMessage(null, EventType.DatabaseUpdate, null);
        public static EntityMessage GC { get; } = new EntityMessage(null, EventType.GC, null);

        public enum EventType
        {
            // system messages
            DestroyEntity, /* Sent whenever the entity is being destroyed */
            FrameEnd, /* Frame has ended, used for resets */
            FrameUpdate, /* Time to do update logic */
            NetworkUpdate, /* Time to to network sync logic */
            DatabaseUpdate, /* Time to do database sync logic */
            GC, /* Collect entity garbage */
            NewSystemMessage,

            // visual messages
            EntityEnteredViewRange,
            EntityLeftViewRange,

            // health
            TookDamage,
            JustDied,
            HealedHealth,
            ForcedMovement,

            // entity
            ParticleEffect,
            NewAnimation,
            NewOverheadText,

            // npc
            DefinitionChange,

            // player
            ChatMessage,
            AppearanceChanged,
            ClientRegionChanged,

            // transform messages
            NewInteractingEntity,
            Move, /* Moving by delta (ie walking or running) */
            PoeSwitch,
            Teleport, /* Forced movement over an arbitrary size of land */
            NewFacingDirection,

            // pathing messages
            BeginMovePath,
            StopMovingAlongMovePath, /* We suddenly stop moving on the current path (direction provider) without actually arriving at the destination */
            ArrivedAtDestination, /* Sent whenever a movement controller's direction provider is done */

            // network messages
            NewPacket,
            NetworkReinitialize, /* The network connection has been reinitialized */
        };

        public EntityMessage([CanBeNull] IEntityComponent sender, EventType ev, [CanBeNull] object data)
        {
            _data = data;
            Sender = sender;
            Event = ev;
        }

        private T AssertCast<T>(EventType expected)
        {
            Debug.Assert(expected == Event);
            return (T) _data;
        }

        private bool AssertTrue(EventType expected)
        {
            Debug.Assert(Event == expected);
            return true;
        }

        public string AsNewOverheadText() => AssertCast<string>(EventType.NewOverheadText);
        public Animation AsNewAnimation() => AssertCast<Animation>(EventType.NewAnimation);
        public ParticleEffect AsParticleEffect() => AssertCast<ParticleEffect>(EventType.ParticleEffect);
        public ForcedMovement AsForcedMovement() => AssertCast<ForcedMovement>(EventType.ForcedMovement);
        public ChatMessage AsChatMessage() => AssertCast<ChatMessage>(EventType.ChatMessage);
        public bool AsAppearanceChanged => AssertTrue(EventType.AppearanceChanged);
        public bool AsGC() => AssertTrue(EventType.GC);

        public int AsDefinitionChange() => AssertCast<int>(EventType.DefinitionChange);

        public EntityHandle AsEntityEnteredViewRange() => AssertCast<EntityHandle>(EventType.EntityEnteredViewRange);
        public EntityHandle AsEntityLeftViewRange() => AssertCast<EntityHandle>(EventType.EntityLeftViewRange);

        public IInteractingEntity AsNewInteractingEntity() =>
            AssertCast<IInteractingEntity>(EventType.NewInteractingEntity);
        public string AsNewSystemMessage() => AssertCast<string>(EventType.NewSystemMessage);

        public (int x, int y) AsClientRegionChanged() => AssertCast<(int, int)>(EventType.ClientRegionChanged);
        public (int x, int y) AsNewFacingDirection() => AssertCast<(int, int)>(EventType.NewFacingDirection);
        public bool AsDestroyEntity() => AssertTrue(EventType.DestroyEntity);

        public PacketMetadata AsNewPacket() => AssertCast<PacketMetadata>(EventType.NewPacket);
        public bool AsNetworkReinitialize() => AssertTrue(EventType.NetworkReinitialize);

        public HitData AsTookDamage() => AssertCast<HitData>(EventType.TookDamage);
        public bool AsJustDied() => AssertTrue(EventType.JustDied);
        public MovementMetadata AsMove() => AssertCast<MovementMetadata>(EventType.Move);
        public int AsHealedHealth() => AssertCast<int>(EventType.HealedHealth);

        public PoeSwitchMessageData AsPoeSwitch() => AssertCast<PoeSwitchMessageData>(EventType.PoeSwitch);
        public TeleportMessageData AsTeleport() => AssertCast<TeleportMessageData>(EventType.Teleport);

        public bool AsBeginMovePath() => AssertTrue(EventType.BeginMovePath);
        public bool AsStopMovingAlongMovePath() => AssertTrue(EventType.StopMovingAlongMovePath);
        public bool AsArrivedAtDestination() => AssertTrue(EventType.ArrivedAtDestination);
    }
}