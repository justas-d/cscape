using System.Collections.Generic;
using CScape.Core.Game.Entities.Message;
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

        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.NewOverheadText:
                {
                    SetFlag(new OverheadForcedTextUpdateFlag(msg.AsNewOverheadText()));
                    break;
                }
                case GameMessage.Type.NewAnimation:
                {
                    SetFlag(new AnimationUpdateFlag(msg.AsNewAnimation()));
                    break;
                }
                case GameMessage.Type.ParticleEffect:
                {
                    SetFlag(new ParticleEffectUpdateFlag(msg.AsParticleEffect()));
                    break;
                }
                case GameMessage.Type.ForcedMovement:
                {
                    SetFlag(new ForcedMovementUpdateFlag(msg.AsForcedMovement()));
                    break;
                }
                case GameMessage.Type.ChatMessage:
                {
                    SetFlag(new PlayerChatUpdateFlag(msg.AsChatMessage()));
                    break;
                }
                case GameMessage.Type.AppearanceChanged:
                {
                    SetFlag(new PlayerAppearanceUpdateFlag());
                    break;
                }
                case GameMessage.Type.TookDamage:
                {
                    SetFlag(new DamageUpdateFlag(msg.AsTookDamage()));
                    break;
                }
                case GameMessage.Type.NewFacingDirection:
                {
                    SetFlag(new FacingCoordinateUpdateFlag(msg.AsNewFacingDirection()));
                    break;
                }
                case GameMessage.Type.NewInteractingEntity:
                {
                    SetFlag(new InteractingEntityUpdateFlag(msg.AsNewInteractingEntity()));
                    break;
                }
                case GameMessage.Type.DefinitionChange:
                {
                    SetFlag(new DefinitionChangeUpdateFlag(msg.AsDefinitionChange()));
                    break;
                }
                case GameMessage.Type.Move:
                {
                    Movement = msg.AsMove();
                    break;
                }
                case GameMessage.Type.Teleport:
                {
                    Reinitialize = true;
                    break;
                }
                case GameMessage.Type.FrameEnd:
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