using System;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    /// <summary>
    /// Handles the syncing of all observables.
    /// </summary>
    public sealed class ObservableSyncMachine : ISyncMachine
    {
        public int Order => SyncMachineConstants.Observer;
        public bool RemoveAfterInitialize { get; } = false;

        public Player LocalPlayer { get; }

        private readonly PlayerObservatory _playerObservatory;

        public PlayerUpdateSyncMachine PlayerSync { get; }
        public NpcUpdateSyncMachine NpcSync { get; }
        public GroundItemSyncMachine ItemSync { get; }

        public ObservableSyncMachine(
            IServiceProvider services,
            [NotNull] Player player, [NotNull] PlayerObservatory playerObservatory)
        {
            _playerObservatory = playerObservatory ?? throw new ArgumentNullException(nameof(playerObservatory));

            LocalPlayer = player ?? throw new ArgumentNullException(nameof(player));

            PlayerSync = new PlayerUpdateSyncMachine(LocalPlayer);
            NpcSync = new NpcUpdateSyncMachine(LocalPlayer);
            ItemSync = new GroundItemSyncMachine(services, LocalPlayer);

            LocalPlayer.Connection.SyncMachines.Add(PlayerSync);
            LocalPlayer.Connection.SyncMachines.Add(NpcSync);
            LocalPlayer.Connection.SyncMachines.Add(ItemSync);
        }

        public void Clear()
        {
            PlayerSync.Clear();
            NpcSync.Clear();
            ItemSync.Clear();
        }

        public void Synchronize(OutBlob stream)
        {
            // iterate over all IObservables in Observatory, sync them.
            foreach (var obs in LocalPlayer.Observatory)
            {
                if (_playerObservatory.PopIsNew(obs))
                {
                    switch (obs)
                    {
                        case Player p:
                            PlayerSync.PushPlayer(p);
                            break;
                        case Npc n:
                            NpcSync.PushNpc(n);
                            break;
                    }
                }

                if (obs is GroundItem i)
                    ItemSync.UpdateItem(i);
            }
        }

        public void OnReinitialize()
        {
            _playerObservatory.ReevaluateSightOverride = true;
            _playerObservatory.Clear();
        }
    }
}
