using CScape.Core.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    public abstract class SyncMachine
    {
        public abstract int Order { get; }

        public abstract void Synchronize([NotNull] OutBlob stream);
    }
}