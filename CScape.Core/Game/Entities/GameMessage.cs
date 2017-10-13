using System.Diagnostics;
using CScape.Core.Game.Entities.FacingData;
using CScape.Core.Game.Entities.InteractingEntity;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class GameMessage
    {
        private readonly object _data;

        [CanBeNull]
        public IEntityComponent Sender { get; }
        public Type Event { get; }

        public static GameMessage FrameEnd { get; } = new GameMessage(null, Type.FrameEnd, null);
        public static GameMessage FrameUpdate { get; } = new GameMessage(null, Type.FrameUpdate, null);
        public static GameMessage NetworkUpdate { get; } = new GameMessage(null, Type.NetworkUpdate, null);
        public static GameMessage DatabaseUpdate { get; } = new GameMessage(null, Type.DatabaseUpdate, null);
        public static GameMessage GC { get; } = new GameMessage(null, Type.GC, null);

        public enum Type
        {
            // system messages
            DestroyEntity, /* Sent whenever the entity is being destroyed */
            FrameEnd, /* Frame has ended, used for resets */
            FrameUpdate, /* Time to do update logic */
            NetworkUpdate, /* Time to to network sync logic */
            DatabaseUpdate, /* Time to do database sync logic */
            GC, /* Collect entity garbage */
            NewSystemMessage,

            // item
            ItemChange,
            EquipmentChange,

            // interface
            NewInterfaceShown,
            InterfaceClosed,
            ButtonClicked,

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

        public GameMessage([CanBeNull] IEntityComponent sender, Type ev, [CanBeNull] object data)
        {
            _data = data;
            Sender = sender;
            Event = ev;
        }

        private T AssertCast<T>(Type expected)
        {
            Debug.Assert(expected == Event);
            return (T) _data;
        }

        public ButtonClick AsButtonClicked() => AssertCast<ButtonClick>(Type.ButtonClicked);
        public ItemChange AsItemChange() => AssertCast<ItemChange>(Type.ItemChange);
        public ItemChange AsEquipmentChange() => AssertCast<ItemChange>(Type.EquipmentChange);

        public InterfaceMetadata AsInterfaceClosed() => AssertCast<InterfaceMetadata>(Type.InterfaceClosed);
        public InterfaceMetadata AsNewInterfaceShown() => AssertCast<InterfaceMetadata>(Type.NewInterfaceShown);

        public string AsNewOverheadText() => AssertCast<string>(Type.NewOverheadText);
        public Animation AsNewAnimation() => AssertCast<Animation>(Type.NewAnimation);
        public ParticleEffect AsParticleEffect() => AssertCast<ParticleEffect>(Type.ParticleEffect);
        public ForcedMovement AsForcedMovement() => AssertCast<ForcedMovement>(Type.ForcedMovement);
        public ChatMessage AsChatMessage() => AssertCast<ChatMessage>(Type.ChatMessage);
        public bool AsAppearanceChanged => AssertTrue(Type.AppearanceChanged);

        public short AsDefinitionChange() => AssertCast<short>(Type.DefinitionChange);

        public EntityHandle AsEntityEnteredViewRange() => AssertCast<EntityHandle>(Type.EntityEnteredViewRange);
        public EntityHandle AsEntityLeftViewRange() => AssertCast<EntityHandle>(Type.EntityLeftViewRange);

        public IInteractingEntity AsNewInteractingEntity() =>
            AssertCast<IInteractingEntity>(Type.NewInteractingEntity);
        public string AsNewSystemMessage() => AssertCast<string>(Type.NewSystemMessage);

        public (int x, int y) AsClientRegionChanged() => AssertCast<(int, int)>(Type.ClientRegionChanged);
        public IFacingData AsNewFacingDirection() => AssertCast<IFacingData>(Type.NewFacingDirection);

        public PacketMetadata AsNewPacket() => AssertCast<PacketMetadata>(Type.NewPacket);

        public HitData AsTookDamage() => AssertCast<HitData>(Type.TookDamage);
        public MovementMetadata AsMove() => AssertCast<MovementMetadata>(Type.Move);
        public int AsHealedHealth() => AssertCast<int>(Type.HealedHealth);

        public PoeSwitchMessageData AsPoeSwitch() => AssertCast<PoeSwitchMessageData>(Type.PoeSwitch);
        public TeleportMessageData AsTeleport() => AssertCast<TeleportMessageData>(Type.Teleport);
    }
}