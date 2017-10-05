using CScape.Core.Game.Entities.InteractingEntity;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entities.Message;
using System.Diagnostics;
using CScape.Core.Data;

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
                case EntityMessage.EventType.Teleport:
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
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class PlayerNetworkSyncComponent : EntityComponent
    {
        public override int Priority { get; } = ComponentConstants.PriorityPlayerUpdate;


        public PlayerNetworkSyncComponent(Entity parent)
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
            var net = Parent.Components.Get<NetworkingComponent>();
            Debug.Assert(net != null);

            var stream = net.OutStream;

            stream.BeginPacket(81);

            stream.BeginBitAccess();

            

            stream.EndPacket();
        }

    }

    public sealed class UpdateWriter
    {
        enum Flags : int
        {
            ForcedMovement = 0x400,
            ParticleEffect = 0x100,
            Animation = 8,
            ForcedText = 4,
            Chat = 0x80,
            InteractEnt = 0x1,
            Appearance = 0x10,
            FacingCoordinate = 0x2,
            PrimaryHit = 0x20,
            SecondaryHit = 0x200,
        }

        public void SetFlag(int flag)
        {
            
        }

        public void Write(OutBlob stream)
        {
            
        }
    }

}