namespace CScape.Core.Game.Entity.Component
{
    public enum ComponentPriority
    {

        Player,
        Npc,
        Networking,
        PacketDispatch,
        Transform,
        TileMovement,
        MovementActionComponent,
        ClientPositionComponent,
        CappedVision,
        EntityWatcher,
        ItemActionDispatchComponent,
        GroundItemComponent,
        InterfaceComponent,
        CombatStatComponent,
        HealthComponent,
        FlagAccumulatorComponent,
        
        Invariant,

        MessageLogComponent,

        RegionSync,
        PlayerSync,
        NpcSync,
        GroundItemSync,

        DebugStatSync,

        InvariantSync,

        MessageSync
    }
}