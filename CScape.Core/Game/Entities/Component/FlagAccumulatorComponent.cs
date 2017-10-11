using System.Collections.Generic;
using CScape.Core.Network.Entity;
using CScape.Core.Network.Entity.Flag;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class FlagAccumulatorComponent : EntityComponent
    {
        private readonly Dictionary<FlagType, IUpdateFlag> _flags
            = new Dictionary<FlagType, IUpdateFlag>();

        public IReadOnlyDictionary<FlagType, IUpdateFlag> Flags => _flags;

        public MovementMetadata Movement { get; private set; }
        public bool Reinitialize { get; private set; }

        public override int Priority { get; }

        public FlagAccumulatorComponent(Entity parent)
            :base(parent)
        {
        
        }

        private void SetFlag(IUpdateFlag flag)
        {
            if (_flags.ContainsKey(flag.Type))
                _flags[flag.Type] = flag;
            else
                _flags.Add(flag.Type, flag);
        }

        public override void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.NewOverheadText:
                {
                    SetFlag(new OverheadForcedTextUpdateFlag(msg.AsNewOverheadText()));
                    break;
                }
                case EntityMessage.EventType.NewAnimation:
                {
                    SetFlag(new AnimationUpdateFlag(msg.AsNewAnimation()));
                    break;
                }
                case EntityMessage.EventType.ParticleEffect:
                {
                    SetFlag(new ParticleEffectUpdateFlag(msg.AsParticleEffect()));
                    break;
                }
                case EntityMessage.EventType.ForcedMovement:
                {
                    SetFlag(new ForcedMovementUpdateFlag(msg.AsForcedMovement()));
                    break;
                }
                case EntityMessage.EventType.ChatMessage:
                {
                    SetFlag(new PlayerChatUpdateFlag(msg.AsChatMessage()));
                    break;
                }
                case EntityMessage.EventType.AppearanceChanged:
                {
                    SetFlag(new PlayerAppearanceUpdateFlag());
                    break;
                }
                case EntityMessage.EventType.TookDamage:
                {
                    SetFlag(new DamageUpdateFlag(msg.AsTookDamage()));
                    break;
                }
                case EntityMessage.EventType.NewFacingDirection:
                {
                    SetFlag(new FacingCoordinateUpdateFlag(msg.AsNewFacingDirection()));
                    break;
                }
                case EntityMessage.EventType.NewInteractingEntity:
                {
                    SetFlag(new InteractingEntityUpdateFlag(msg.AsNewInteractingEntity()));
                    break;
                }
                case EntityMessage.EventType.DefinitionChange:
                {
                    SetFlag(new DefinitionChangeUpdateFlag(msg.AsDefinitionChange()));
                    break;
                }
                case EntityMessage.EventType.Move:
                {
                    Movement = msg.AsMove();
                    break;
                }
                case EntityMessage.EventType.Teleport:
                {
                    Reinitialize = true;
                    break;
                }
                case EntityMessage.EventType.FrameEnd:
                {
                    _flags.Clear();
                    Reinitialize = false;
                    Movement = null;
                    break;
                }
            }
        }
    }
}