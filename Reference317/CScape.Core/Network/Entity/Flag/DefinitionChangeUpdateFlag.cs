using CScape.Models.Data;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class DefinitionChangeUpdateFlag : IUpdateFlag
    {
        public short Definition { get; }

        public FlagType Type => FlagType.DefinitionChange;

        public DefinitionChangeUpdateFlag(short definition)
        {
            Definition = definition;
        }

        public void Write(OutBlob stream)
        {
            stream.Write16(Definition);
        }
    }
}