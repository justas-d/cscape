using System;
using CScape.Data;
using CScape.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    /// <summary>
    /// Handles the syncing of all observables.
    /// </summary>
    public sealed class ObservableSyncMachine : SyncMachine
    {
        public override int Order => SyncMachineConstants.Observer;

        public Player LocalPlayer { get; }

        private readonly PlayerObservatory _playerObservatory;
        private readonly PlayerUpdateSyncMachine _playerSync;

        public ObservableSyncMachine([NotNull] Player player, [NotNull] PlayerObservatory playerObservatory) : base(player.Server)
        {
            _playerObservatory = playerObservatory ?? throw new ArgumentNullException(nameof(playerObservatory));
            LocalPlayer = player ?? throw new ArgumentNullException(nameof(player));

            _playerSync = new PlayerUpdateSyncMachine(player.Server, LocalPlayer);
            LocalPlayer.Connection.SyncMachines.Add(_playerSync);
        }

        public bool IsLocalPlayer(Player player)
        {
            return LocalPlayer.Equals(player);
        }

        public void Clear()
            => _playerSync.Clear();

        public void PushToPlayerSyncMachine(Player player)
            => _playerSync.PushPlayer(player);

        // todo : public void PushToNpcSyncMachine(Npc npc)

        public override void Synchronize(OutBlob stream)
        {
            // iterate over all IObservables in Observatory, sync them.
            foreach (var obs in LocalPlayer.Observatory)
                obs.SyncTo(this, stream, _playerObservatory.PopIsNew(obs));
        }
    }
}