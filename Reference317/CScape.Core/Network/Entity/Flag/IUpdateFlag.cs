using CScape.Models.Data;

namespace CScape.Core.Network.Entity.Flag
{
    public interface IUpdateFlag
    {
        FlagType Type { get; }
        void Write(OutBlob stream);
    }
}