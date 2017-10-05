using CScape.Core.Game.Entities.InteractingEntity;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class FlagAccumulatorComponent : EntityComponent
    {
        public HitData Damage { get; private set; }
        public (int x, int y)? FacingDir { get; private set; }
        public IInteractingEntity InteractingEntity { get; private set; }
        public int? DefinitionChange { get; private set; }
        public bool Reinitialize { get; private set; }

        /* TODO:
         * 
         * ForcedMovement
         * ParticleEffect
         * Animation
         * ForcedText
         * Chat
         * Appearance
         * 
         */

        public override int Priority { get; }

        public FlagAccumulatorComponent(Entity parent)
            :base(parent)
        {
        
        }
       
        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.TookDamage:
                {
                    Damage = msg.AsTookDamage();
                    break;
                }
                case EntityMessage.EventType.NewFacingDirection:
                {
                    FacingDir = msg.AsNewFacingDirection();
                    break;
                }
                case EntityMessage.EventType.NewInteractingEntity:
                {
                    InteractingEntity = msg.AsNewInteractingEntity();
                    break;
                }
                case EntityMessage.EventType.DefinitionChange:
                {
                    DefinitionChange = msg.AsDefinitionChange();
                    break;
                }
                case EntityMessage.EventType.NeedsUpdateReiniaialize:
                {
                    Reinitialize = true;
                    break;
                }
                case EntityMessage.EventType.FrameEnd:
                {
                    Damage = null;
                    FacingDir = null;
                    InteractingEntity = null;
                    DefinitionChange = null;
                    Reinitialize = false;
                    break;
                }
            }
        }
    }


    /// <summary>
    /// Responsible for syncing every visible player entity to the network.
    /// </summary>
    public sealed class PlayerSyncFragment : EntityComponent
    {
        public override int Priority { get; } = ComponentConstants.PriorityPlayerUpdate;


        public PlayerSyncFragment(Entity parent)
            :base(parent)
        {
            
        }
        
        public override void ReceiveMessage(EntityMessage msg)
        {
            if (msg.Event == EntityMessage.EventType.NetworkUpdate)
            {
                Sync();
            }
        }

        private void Sync()
        {
            
        }

    }
}