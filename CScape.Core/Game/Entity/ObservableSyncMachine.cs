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
        private readonly PlayerUpdateSyncMachine _playerSync;
        private readonly NpcUpdateSyncMachine _npcSync;

        public ObservableSyncMachine([NotNull] Player player, [NotNull] PlayerObservatory playerObservatory)
        {
            _playerObservatory = playerObservatory ?? throw new ArgumentNullException(nameof(playerObservatory));
            LocalPlayer = player ?? throw new ArgumentNullException(nameof(player));

            _playerSync = new PlayerUpdateSyncMachine(LocalPlayer);
            _npcSync = new NpcUpdateSyncMachine(LocalPlayer);

            LocalPlayer.Connection.SyncMachines.Add(_playerSync);
            LocalPlayer.Connection.SyncMachines.Add(_npcSync);
        }

        public void Clear()
            => _playerSync.Clear();

        public void PushToPlayerSyncMachine(Player player)
            => _playerSync.PushPlayer(player);

        public void PushToNpcSyncMachine(Npc npc)
            => _npcSync.PushNpc(npc);

        public void Synchronize(OutBlob stream)
        {
            // iterate over all IObservables in Observatory, sync them.
            foreach (var obs in LocalPlayer.Observatory)
                obs.SyncTo(this, stream, _playerObservatory.PopIsNew(obs));
        }
    }
}