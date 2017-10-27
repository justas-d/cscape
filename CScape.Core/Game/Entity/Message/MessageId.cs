namespace CScape.Core.Game.Entity.Message
{
    public enum MessageId : int
    {
        // system messages
        NetworkUpdate, /* Time to to network sync logic */
        DatabaseUpdate, /* Time to do database sync logic */
        NewSystemMessage,

        // skill
        GainExperience,
        LevelUp,

        // item
        ItemChange,
        EquipmentChange,
        ItemAction,
        ItemOnItemAction,
        GroundItemAmountUpdate,

        // interface
        NewInterfaceShown,
        InterfaceClosed,
        ButtonClicked,
        InterfaceUpdate,

        // visual messages
        EntityEnteredViewRange,
        EntityLeftViewRange,

        // health
        JustDied,
        HealthChanged,
        EatHealedHealth,
        TookDamageLostHealth,
        MaxHealthChanged,

        // entity
        ParticleEffect,
        NewAnimation,
        NewOverheadText,
        ForcedMovement,
        // TODO : overheads

        // npc
        DefinitionChange,

        // player
        ChatMessage,
        UpdatePlayerAppearance,
        ClientRegionChanged,
        PlayerInitialize, // signals to entities that this player needs to be initialized.

        // transform messages
        NewInteractingEntity,
        Move, /* Moving by delta (ie walking or running) */
        PoeSwitch,
        Teleport, /* Forced movement over an arbitrary size of land */
        NewFacingDirection,

        // pathing messages
        NewPlayerFollowTarget,
        BeginMovePath,
        StopMovingAlongMovePath, /* We suddenly stop moving on the current path (direction provider) without actually arriving at the destination */
        ArrivedAtDestination, /* Sent whenever a movement controller's direction provider is done */

        // network messages
        NewPacket,
        NetworkReinitialize, /* The network connection has been reinitialized */
    }
}