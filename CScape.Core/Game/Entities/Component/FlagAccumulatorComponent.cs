using System.Collections.Generic;
using CScape.Core.Extensions;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Network.Entity;
using CScape.Core.Network.Entity.Flag;
using CScape.Models.Data;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class FlagAccumulatorComponent : EntityComponent
    {
        private readonly Dictionary<FlagType, IUpdateFlag> _flags
            = new Dictionary<FlagType, IUpdateFlag>();

        public IReadOnlyDictionary<FlagType, IUpdateFlag> Flags => _flags;

        public const int MaxAppearanceUpdateSize = 64;

        private readonly Blob _appearanceCache = new Blob(MaxAppearanceUpdateSize);

        [CanBeNull]
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

        public void HandleAppearanceMessage(NewPlayerAppearanceMessage msg)
        {
            var cache = _appearanceCache;
            var app = msg.Appearance;

            const int equipSlotSize = 12;

            const int plrObjMagic = 0x100;
            const int itemMagic = 0x200;


            Parent.SystemMessage("Invalidating and rewriting appearance cache.");

            cache.ResetWrite();

            var sizePh = cache.Placeholder(1);

            cache.Write((byte)app.Gender);
            // TODO : overheads
            cache.Write((byte)app.Overhead);

            /* 
             * todo : some equipped items conflict with body parts 
             * write body model if chest doesn't conceal the body
             * write head model if head item doesn't fully conceal the head.
             * write beard model if head item doesn't fully conceal the head.
             */


            void Write(short value)
            {
                    
            }

            for (var i = 0; i < equipSlotSize; i++)
            {
                // todo : write equipment
                if (upd.Player.Appearance[i] != null)
                    cache.Write16((short) (upd.Player.Appearance[i].Value + plrObjMagic));
                else
                    cache.Write(0);
            }

            cache.Write(upd.Player.Appearance.HairColor);
            cache.Write(upd.Player.Appearance.TorsoColor);
            cache.Write(upd.Player.Appearance.LegColor);
            cache.Write(upd.Player.Appearance.FeetColor);
            cache.Write(upd.Player.Appearance.SkinColor);

            // upd.Player animation indices
            cache.Write16(0x328); // standAnimIndex
            cache.Write16(0x337); // standTurnAnimIndex
            cache.Write16(0x333); // walkAnimIndex
            cache.Write16(0x334); // turn180AnimIndex
            cache.Write16(0x335); // turn90CWAnimIndex
            cache.Write16(0x336); // turn90CCWAnimIndex
            cache.Write16(0x338); // runAnimIndex

            cache.Write64(Utils.StringToLong(upd.Player.Username));
            cache.Write(3); // todo : cmb
            cache.Write16(0); // ...skill???

            sizePh.WriteSize();
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