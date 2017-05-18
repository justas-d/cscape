using System;
using CScape.Core.Data;
using CScape.Core.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity
{
    /// <summary>
    /// Handles the syncing of all observables.
    /// </summary>
    public sealed class ObservableSyncMachine : ISyncMachine
    {
        public int Order => SyncMachineConstants.Observer;

        public Player LocalPlayer { get; }

        private readonly PlayerObservatory _playerObservatory;
        public PlayerUpdateSyncMachine PlayerSync { get; }
        public NpcUpdateSyncMachine NpcSync { get; }

        public ObservableSyncMachine([NotNull] Player player, [NotNull] PlayerObservatory playerObservatory)
        {
            _playerObservatory = playerObservatory ?? throw new ArgumentNullException(nameof(playerObservatory));
            LocalPlayer = player ?? throw new ArgumentNullException(nameof(player));

            PlayerSync = new PlayerUpdateSyncMachine(LocalPlayer);
            NpcSync = new NpcUpdateSyncMachine(LocalPlayer);

            LocalPlayer.Connection.SyncMachines.Add(PlayerSync);
            LocalPlayer.Connection.SyncMachines.Add(NpcSync);
        }

        public void Clear()
            => PlayerSync.Clear();

        public void Synchronize(OutBlob stream)
        {
            // iterate over all IObservables in Observatory, sync them.
            foreach (var obs in LocalPlayer.Observatory)
                obs.SyncTo(this, stream, _playerObservatory.PopIsNew(obs));
        }
    }
}