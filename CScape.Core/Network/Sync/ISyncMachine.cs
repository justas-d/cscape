using CScape.Core.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    public interface ISyncMachine
    {
        int Order { get; }
        bool RemoveAfterInitialize { get; }

        void Synchronize([NotNull] OutBlob stream);

        /// <summary>
        /// Called whenever the owning player's socket context reinitializes.
        /// Modifying sync machines during this proceedure is undefined and will lead to exceptions.
        /// </summary>
        void OnReinitialize();
    }
}