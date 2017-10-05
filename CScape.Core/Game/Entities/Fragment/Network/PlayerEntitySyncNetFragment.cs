using System;
using CScape.Core.Game.Entities.Fragment.Component;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;

namespace CScape.Core.Game.Entities.Fragment.Network
{
    public sealed class FlagAccumulatorComponent : IEntityComponent
    {
        [Flags]
        enum InternalFlags
        {
            ForcedMovement,
            ParticleEffect,
            Animation,
            ForcedText,
            Chat,
            InteractingEntity,
            Appearance,
            FacingCoordinate,
            PrimaryHit,
            SecondaryHit
        }

        private InternalFlags _flags;
        private HitData _damage;
        private (int x, int y) _facingDir;
        private IInteractingEntity _interactingEntity;

        public Entity Parent { get; }
        public int Priority { get; }

        public FlagAccumulatorComponent(Entity parent)
        {
            Parent = parent;
        }
        
        public void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.TookDamage:
                {
                    _flags |= InternalFlags.PrimaryHit;
                    _damage = msg.AsTookDamage();
                    break;
                }
                case EntityMessage.EventType.NewFacingDirection:
                {
                    _flags |= InternalFlags.FacingCoordinate;
                    _facingDir = msg.AsNewFacingDirection();
                    break;
                }
                case EntityMessage.EventType.NewInteractingEntity:
                {
                    _flags |= InternalFlags.InteractingEntity;
                    _interactingEntity = msg.AsNewInteractingEntity();
                    break;
                }
            }
        }

        public void Update(IMainLoop loop) { }
    }


    /// <summary>
    /// Responsible for syncing every visible player entity to the network.
    /// </summary>
    public sealed class PlayerSyncFragment<T> : IEntityNetFragment
    {
        public Entity Parent { get; }
        public int Priority { get; } = NetFragConstants.PriorityPlayerUpdate;


        public PlayerSyncFragment(Entity parent)
        {
            Parent = parent;
        }
        
        public void ReceiveMessage(EntityMessage msg)
        {
          
        }

        public void Update(IMainLoop loop, NetworkingComponent network)
        {

        }
    }
}