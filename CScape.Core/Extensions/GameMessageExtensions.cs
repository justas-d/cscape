using System.Diagnostics;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Message;

namespace CScape.Core.Extensions
{
    public static class GameMessageExtensions
    {
        private static T AssertCast<T>(IGameMessage msg, MessageId id)
            where T : class, IGameMessage
        {
            Debug.Assert(msg.EventId == (int)id);
            var val = msg as T;
            Debug.Assert(val != null);
            return val;
        }

        public static bool Is(this IGameMessage msg, MessageId id) => msg.EventId == (int)id;

        public static PlayerAppearanceMessage AsPlayerAppearance(this IGameMessage msg) =>
            AssertCast<PlayerAppearanceMessage>(msg, MessageId.UpdatePlayerAppearance);

        public static PacketMessage AsNewPacket(this IGameMessage msg) =>
            AssertCast<PacketMessage>(msg, MessageId.NewPacket);

        public static SystemMessage AsSystemMessage(this IGameMessage msg) =>
            AssertCast<SystemMessage>(msg, MessageId.NewSystemMessage);

        public static ExperienceGainMessage AsExperienceGain(this IGameMessage msg) =>
            AssertCast<ExperienceGainMessage>(msg, MessageId.GainExperience);

        public static LevelUpMessage AsLevelUp(this IGameMessage msg) =>
            AssertCast<LevelUpMessage>(msg, MessageId.LevelUp);

        public static ItemChangeMessage AsItemChange(this IGameMessage msg) =>
            AssertCast<ItemChangeMessage>(msg, MessageId.ItemChange);

        public static ItemChangeMessage AsEquipmentChange(this IGameMessage msg) =>
            AssertCast<ItemChangeMessage>(msg, MessageId.EquipmentChange);

        public static ItemActionMessage AsItemAction(this IGameMessage msg) =>
            AssertCast<ItemActionMessage>(msg, MessageId.ItemAction);

        public static ItemOnItemMessage AsItemOnItemAction(this IGameMessage msg) =>
            AssertCast<ItemOnItemMessage>(msg, MessageId.ItemOnItemAction);

        public static GroundItemMessage AsGroundItemAmountUpdate(this IGameMessage msg) =>
            AssertCast<GroundItemMessage>(msg, MessageId.GroundItemAmountUpdate);

        public static InterfaceMessage AsNewInterfaceShown(this IGameMessage msg) =>
            AssertCast<InterfaceMessage>(msg, MessageId.NewInterfaceShown);

        public static InterfaceMessage AsInterfaceClosed(this IGameMessage msg) =>
            AssertCast<InterfaceMessage>(msg, MessageId.InterfaceClosed);

        public static InterfaceMessage AsInterfaceUpdate(this IGameMessage msg) =>
            AssertCast<InterfaceMessage>(msg, MessageId.InterfaceUpdate);

        public static ButtonClickMessage AsButtonClicked(this IGameMessage msg) =>
            AssertCast<ButtonClickMessage>(msg, MessageId.ButtonClicked);

        public static EntityMessage AsEntityEnteredViewRange(this IGameMessage msg) =>
            AssertCast<EntityMessage>(msg, MessageId.EntityEnteredViewRange);

        public static EntityMessage AsEntityLeftViewRange(this IGameMessage msg) =>
            AssertCast<EntityMessage>(msg, MessageId.EntityLeftViewRange);

        public static HealthUpdateMessage AsHealthChange(this IGameMessage msg) =>
            AssertCast<HealthUpdateMessage>(msg, MessageId.HealthChanged);

        public static TakeDamageMessage AsTookDamangeLostHealth(this IGameMessage msg) =>
            AssertCast<TakeDamageMessage>(msg, MessageId.TookDamageLostHealth);

        public static EatHealedHealthMessage AsEatHealed(this IGameMessage msg) =>
            AssertCast<EatHealedHealthMessage>(msg, MessageId.EatHealedHealth);

        public static ForcedMovementMessage AsForcedMovement(this IGameMessage msg) =>
            AssertCast<ForcedMovementMessage>(msg, MessageId.ForcedMovement);

        public static ParticleEffectMessage AsParticleEffect(this IGameMessage msg) =>
            AssertCast<ParticleEffectMessage>(msg, MessageId.ParticleEffect);

        public static AnimationMessage AsNewAnimation(this IGameMessage msg) =>
            AssertCast<AnimationMessage>(msg, MessageId.NewAnimation);

        public static OverheadTextMessage AsNewOverheadText(this IGameMessage msg) =>
            AssertCast<OverheadTextMessage>(msg, MessageId.NewOverheadText);

        public static DefinitionChangeMessage AsDefinitionChange(this IGameMessage msg) =>
            AssertCast<DefinitionChangeMessage>(msg, MessageId.DefinitionChange);

        public static ChatMessageMessage AsChatMessage(this IGameMessage msg) =>
            AssertCast<ChatMessageMessage>(msg, MessageId.ChatMessage);

        public static PositionMessage AsClientRegionChange(this IGameMessage msg) =>
            AssertCast<PositionMessage>(msg, MessageId.ClientRegionChanged);

        public static InteractingEntityMessage AsNewInteractingEntity(this IGameMessage msg) =>
            AssertCast<InteractingEntityMessage>(msg, MessageId.NewInteractingEntity);

        public static MoveMessage AsMove(this IGameMessage msg) =>
            AssertCast<MoveMessage>(msg, MessageId.Move);

        public static PoeSwitchMessage AsPoeSwitch(this IGameMessage msg) =>
            AssertCast<PoeSwitchMessage>(msg, MessageId.PoeSwitch);

        public static TeleportMessage AsTeleport(this IGameMessage msg) =>
            AssertCast<TeleportMessage>(msg, MessageId.Teleport);

        public static FacingDirectionMessage AsNewFacingDirection(this IGameMessage msg) =>
            AssertCast<FacingDirectionMessage>(msg, MessageId.NewFacingDirection);

        public static EntityMessage AsNewPlayerFollowTarget(this IGameMessage msg) =>
            AssertCast<EntityMessage>(msg, MessageId.NewPlayerFollowTarget);
    }
}