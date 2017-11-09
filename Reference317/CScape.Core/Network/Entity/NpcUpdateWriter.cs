using CScape.Core.Game.Entity.Component;
using CScape.Core.Network.Entity.Utility;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity
{
    public class NpcUpdateWriter : UpdateWriter
    {
        public NpcUpdateWriter([NotNull] FlagAccumulatorComponent accum) : base(accum)
        {
        }

        public override void Write(OutBlob stream)
        {
            var header = (NpcFlag)GetHeader(f => (int)f.ToNpc());
            if (header == 0) return;

            // header
            stream.Write((byte)header);
            
            // flags
            if ((header & NpcFlag.Animation) != 0)
            {
                GetFlag(FlagType.Animation).Write(stream);
            }
            if ((header & NpcFlag.PrimaryHit) != 0 ||
                (header & NpcFlag.SecondaryHit) != 0)
            {
                GetFlag(FlagType.Damage).Write(stream);
            }

            if ((header & NpcFlag.ParticleEffect) != 0)
            {
                GetFlag(FlagType.ParticleEffect).Write(stream);
            }

            if ((header & NpcFlag.InteractingEntity) != 0)
            {
                GetFlag(FlagType.InteractingEntity).Write(stream);
            }
            if ((header & NpcFlag.Text) != 0)
            {
                GetFlag(FlagType.OverheadText).Write(stream);
            }
            if ((header & NpcFlag.Definition) != 0)
            {
                GetFlag(FlagType.DefinitionChange).Write(stream);
            }
            if ((header & NpcFlag.FacingCoordinate) != 0)
            {
                GetFlag(FlagType.FacingDir).Write(stream);
            }
        }

        public override bool NeedsUpdate()
        {
            var header = (NpcFlag)GetHeader(f => (int)f.ToNpc());
            return header != 0;
        }
    }
}