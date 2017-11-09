namespace CScape.Core.Game.Entity.Message
{
    public enum MessageId : int
    {
        FrameBegin,
        FrameEnd,

        /// <summary>
        /// Clean up dead entities
        /// </summary>
        GC,

        /// <summary>
        /// The entity is going to be dead next frame
        /// </summary>
        QueuedForDeath,
        NearbyEntityQueuedForDeath,

        NewSystemMessage,
        Initialize,

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

        // transform messages
        NewInteractingEntity,
        Move, /* Moving by delta (ie walking or running) */
        PoeSwitch,
        Teleport, /* Forced movement over an arbitrary size of land */
        NewFacingDirection,
        SyncLocalsToGlobals,

        // pathing messages
        NewPlayerFollowTarget,
        BeginMovePath,
        StopMovingAlongMovePath, /* We suddenly stop moving on the current path (direction provider) without actually arriving at the destination */
        ArrivedAtDestination, /* Sent whenever a movement controller's direction provider is done */

        // network messages
        NetworkPrepare, /* prepare packets */
        NetworkSync, /* sync state with clients */
        NewPacket,
        NetworkReinitialize, /* The network connection has been reinitialized */
    }
}