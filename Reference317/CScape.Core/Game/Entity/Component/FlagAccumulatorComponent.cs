using System.Collections.Generic;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Entity;
using CScape.Core.Network.Entity.Flag;
using CScape.Core.Network.Entity.Utility;
using CScape.Models.Data;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class FlagAccumulatorComponent : EntityComponent
    {
        private readonly Dictionary<FlagType, IUpdateFlag> _flags
            = new Dictionary<FlagType, IUpdateFlag>();

        public IReadOnlyDictionary<FlagType, IUpdateFlag> Flags => _flags;

        // TODO : different flag accumulators for NPCs and players
        private readonly PlayerAppearanceSerialier _appearanceSerialier = new PlayerAppearanceSerialier();

        [CanBeNull]
        public MoveMessage Movement { get; private set; }
        public bool Reinitialize { get; private set; }

        public override int Priority => (int)ComponentPriority.FlagAccumulatorComponent;

        public FlagAccumulatorComponent(IEntity parent)
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

        private void HandleAppearanceMessage(PlayerAppearanceMessage msg)
        {
            Parent.SystemMessage("Invalidating and rewriting appearance cache.",
                CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Network);

            _appearanceSerialier.SerializeNewAppearance(msg.Username, msg.Appearance, msg.Equipment);
            SetFlag(CreateAppearanceUpdateFlag());
        }

        public PlayerAppearanceUpdateFlag CreateAppearanceUpdateFlag()
        {
            return new PlayerAppearanceUpdateFlag(_appearanceSerialier.SerializedAppearance);
        }

        private void Reset()
        {
            _flags.Clear();
            Reinitialize = false;
            Movement = null;
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
                case (int)MessageId.UpdatePlayerAppearance:
                {
                    HandleAppearanceMessage(msg.AsPlayerAppearance());
                    break;
                }
                case (int)MessageId.TookDamageLostHealth:
                {
                    SetFlag(new DamageUpdateFlag(msg.AsTookDamangeLostHealth()));
                    break;
                }
                case (int)MessageId.NewFacingDirection:
                {
                    SetFlag(new FacingCoordinateUpdateFlag(msg.AsNewFacingDirection().FacingState));
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
                case (int)MessageId.FrameEnd:
                {
                    Reset();
                    break;
                }
            }
        }
    }
}