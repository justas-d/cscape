namespace CScape.Core.Game.Entity.Component
{
    public enum ComponentPriority
    {

        Player,
        Npc,
        Networking,
        PacketDispatcher,
        Transform,
        TileMovement,
        MovementActionComponent,
        ClientPositionComponent,
        VisionComponent,
        ItemActionDispatchComponent,
        GroundItemComponent,
        InterfaceComponent,
        CombatStatComponent,
        HealthComponent,
        FlagAccumulatorComponent,
        
        Invariant,

        MessageLogComponent,
    }
}