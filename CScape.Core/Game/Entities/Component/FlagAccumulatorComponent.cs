using System.Collections.Generic;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Network.Entity;
using CScape.Core.Network.Entity.Flag;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class FlagAccumulatorComponent : EntityComponent
    {
        private readonly Dictionary<FlagType, IUpdateFlag> _flags
            = new Dictionary<FlagType, IUpdateFlag>();

        public IReadOnlyDictionary<FlagType, IUpdateFlag> Flags => _flags;

        public MoveMessage Movement { get; private set; }
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

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.NewOverheadText:
                {
                    SetFlag(new OverheadForcedTextUpdateFlag(msg.AsNewOverheadText().Message));
                    break;
                }
                case (int)MessageId.NewAnimation:
                {
                    SetFlag(new AnimationUpdateFlag(msg.AsNewAnimation().Animation));
                    break;
                }
                case (int)MessageId.ParticleEffect:
                {
                    SetFlag(new ParticleEffectUpdateFlag(msg.AsParticleEffect().Effect));
                    break;
                }
                case (int)MessageId.ForcedMovement:
                {
                    SetFlag(new ForcedMovementUpdateFlag(msg.AsForcedMovement().Movement));
                    break;
                }
                case (int)MessageId.ChatMessage:
                {
                    SetFlag(new PlayerChatUpdateFlag(msg.AsChatMessage().Chat));
                    break;
                }
                case (int)MessageId.AppearanceChanged:
                {
                    SetFlag(new PlayerAppearanceUpdateFlag());
                    break;
                }
                case (int)MessageId.TookDamageLostHealth:
                {
                    SetFlag(new DamageUpdateFlag(msg.AsTookDamangeLostHealth()));
                    break;
                }
                case (int)MessageId.NewFacingDirection:
                {
                    SetFlag(new FacingCoordinateUpdateFlag(msg.AsNewFacingDirection().FacingData));
                    break;
                }
                case (int)MessageId.NewInteractingEntity:
                {
                    SetFlag(new InteractingEntityUpdateFlag(msg.AsNewInteractingEntity().Interacting));
                    break;
                }
                case (int)MessageId.DefinitionChange:
                {
                    SetFlag(new DefinitionChangeUpdateFlag(msg.AsDefinitionChange().Definition));
                    break;
                }
                case (int)MessageId.Move:
                {
                    Movement = msg.AsMove();
                    break;
                }
                case (int)MessageId.Teleport:
                {
                    Reinitialize = true;
                    break;
                }
                case SysMessage.FrameEnd:
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