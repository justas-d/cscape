using CScape.Core.Game.Entity.Component;
using CScape.Core.Network.Entity.Utility;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity
{
    public class PlayerUpdateWriter : UpdateWriter
    {
        public PlayerUpdateWriter([NotNull] FlagAccumulatorComponent accum) : base(accum)
        {
        }

        public override void Write(OutBlob stream)
        {
            var header = (PlayerFlag)GetHeader(f => (int)f.ToPlayer());
            if (header == 0) return;

            // write header
            stream.Write((byte)header);
            stream.Write((byte)((short)header >> 8));

            // write flags
            if ((header & PlayerFlag.ForcedMovement) != 0)
            {
                GetFlag(FlagType.ForcedMovement).Write(stream);
            }
            if ((header & PlayerFlag.ParticleEffect) != 0)
            {
                GetFlag(FlagType.ParticleEffect).Write(stream);
            }
            if ((header & PlayerFlag.Animation) != 0)
            {
                GetFlag(FlagType.Animation).Write(stream);
            }
            if ((header & PlayerFlag.ForcedText) != 0)
            {
                GetFlag(FlagType.OverheadText).Write(stream);
            }
            if ((header & PlayerFlag.Chat) != 0)
            {
                GetFlag(FlagType.ChatMessage).Write(stream);
            }
            if ((header & PlayerFlag.InteractEnt) != 0)
            {
                GetFlag(FlagType.InteractingEntity).Write(stream);
            }
            if ((header & PlayerFlag.Appearance) != 0)
            {
                GetFlag(FlagType.Appearance).Write(stream);
            }
            if ((header & PlayerFlag.FacingCoordinate) != 0)
            {
                GetFlag(FlagType.FacingDir).Write(stream);
            }
            if ((header & PlayerFlag.PrimaryHit) != 0 ||
                (header & PlayerFlag.SecondaryHit) != 0)
            {
                GetFlag(FlagType.Damage).Write(stream);
            }
        }

        public override bool NeedsUpdate()
        {
            var header = (PlayerFlag)GetHeader(f => (int)f.ToPlayer());
            return header != 0;
        }
    }
}