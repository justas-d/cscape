using JetBrains.Annotations;

namespace CScape.Network.Sync
{
    public abstract class SyncMachine
    {
        public abstract int Order { get; }

        public GameServer Server { get; }

        protected SyncMachine(GameServer server)
        {
            Server = server;
        }

        public abstract void Synchronize([NotNull] OutBlob stream);
    }
}