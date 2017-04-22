using CScape.Network.Sync;

namespace CScape.Game.Entity
{
    /// <summary>
    /// Handles the syncing of all observables.
    /// </summary>
    public sealed class ObservableSyncMachine : SyncMachine
    {
        public override int Order => Constant.SyncMachineOrder.Observer;

        public Player LocalPlayer { get; }

        private readonly PlayerUpdateSyncMachine _playerSync;

        public ObservableSyncMachine(GameServer server, Player player) : base(server)
        {
            LocalPlayer = player;

            _playerSync = new PlayerUpdateSyncMachine(server);
            LocalPlayer.Connection.SyncMachines.Add(_playerSync);
        }

        public bool IsLocalPlayer(Player player)
        {
            return LocalPlayer.Equals(player);
        }

        public void Clear()
            => _playerSync.Clear(LocalPlayer);

        public void PushToPlayerSyncMachine(Player player)
            => _playerSync.PushPlayer(player, LocalPlayer.Equals(player));

        // todo : public void PushToNpcSyncMachine(Npc npc)

        public override void Synchronize(Blob stream)
        {
            // iterate over all IObservables in Observatory, sync them.
            foreach (var obs in LocalPlayer.Observatory)
            {
                obs.Observable.SyncObservable(this, stream, obs.IsNew);
                obs.IsNew = false;
            }
        }
    }
}