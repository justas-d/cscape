using CScape.Core.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    public interface ISyncMachine
    {
        int Order { get; }
        void Synchronize([NotNull] OutBlob stream);
    }
}