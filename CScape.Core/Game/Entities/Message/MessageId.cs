using System.Reflection;

namespace CScape.Core.Game.Entities
{
    public static class MessageId
    {
        // system messages
        public static int NetworkUpdate { get; private set; }
        public static int DatabaseUpdate { get; private set; }
        public static int NewSystemMessage { get; private set; }
        public static int ExperienceGain { get; private set; }
        public static int LevelUp { get; private set; }
        public static int ItemChange { get; private set; }
        public static int EquipmentChange { get; private set; }
        public static int ItemAction { get; private set; }
        public static int GroundItemAmountUpdate { get; private set; }
        public static int NewInterfaceShown { get; private set; }
        public static int InterfaceClosed { get; private set; }
        public static int InterfaceUpdate { get; private set; }
        public static int ButtonClicked { get; private set; }
        public static int EntityEnteredViewRange { get; private set; }
        public static int EntityLeftViewRange { get; private set; }
        public static int TookDamage { get; private set; }
        // TODO : send out AsHealthUpdate
        public static int HealthUpdate { get; private set; }
        public static int JustDied { get; private set; }
        public static int ForcedMovement { get; private set; }
        public static int ParticleEffect { get; private set; }
        public static int NewAnimation { get; private set; }
        public static int NewOverheadText { get; private set; }
        public static int DefinitionChange { get; private set; }
        // TODO : implement AppearanceChanged event
        public static int ChatMessage { get; private set; }
        public static int AppearanceChanged { get; private set; }
        public static int ClientRegionChanged { get; private set; }
        public static int NewInteractingEntity { get; private set; }
        public static int Move { get; private set; }
        public static int PoeSwitch { get; private set; }
        public static int Teleport { get; private set; }
        public static int NewFacingDirection { get; private set; }
        public static int NewPlayerFollowTarget { get; private set; }
        public static int BeginMovePath { get; private set; }
        public static int StopMovingAlongMovePath { get; private set; }
        public static int ArrivedAtDestination { get; private set; }
        public static int NewPacket { get; private set; }
        public static int NetworkReinitialize { get; private set; }

        static MessageId()
        {
            // maintaining the above properties by hand is tedious and error prone
            // so we do it using reflection
            var curId = 0;
            foreach(var prop in typeof(MessageId).GetRuntimeFields())
                prop.SetValue(null, curId++);
        }
    }
}