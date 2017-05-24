using System;
using System.Collections.Generic;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    public sealed class GroundItemSyncMachine : ISyncMachine
    {
        private readonly Player _local;
        public int Order => SyncMachineConstants.GroundItemSync;
        public bool RemoveAfterInitialize => false;

        // keeps track of what items have we sent initial update packets for
        private HashSet<uint> _tracked = new HashSet<uint>();
        private List<IPacket> _queuedNewItemPackets = new List<IPacket>();

        public GroundItemSyncMachine(Player local)
        {
            _local = local;
        }

        // todo : keep track of 100% synced items
        public void UpdateItem(GroundItem item)
        {
            // figure out if the item needs to be synced
            // figure out what update the item needs
            // don't do shit if item is synced.
            // assign a local region x y to item

            // get item local coords from the perspective of the player's client transform
            var itemLocal = (item.Transform.X - _local.ClientTransform.Base.x, item.Transform.Y - _local.ClientTransform.Base.y);

            // lock these locals to the client 8x8 region grid
            var region = (itemLocal.Item1 >> 3, itemLocal.Item2 >> 3);

            // get the offsets by finding the remainder left by the local snap to region grid
            var offsets = (itemLocal.Item1 % 8, itemLocal.Item2 % 8);

            _local.DebugMsg($"(ITEM) update local: ({itemLocal.Item1} {itemLocal.Item2}) region: ({region.Item1} {region.Item2}) offset: ({offsets.Item1} {offsets.Item2})", ref _local.DebugEntitySync);

            // create update, sort it into a bucket labeled after the 8x8 local region coords
        }

        public void Synchronize(OutBlob stream)
        {
        }

        public void OnReinitialize()
        {

        }
    }

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

        private readonly ILogger _log;

        public ObservableSyncMachine(
            IServiceProvider services,
            [NotNull] Player player, [NotNull] PlayerObservatory playerObservatory)
        {
            _log = services.ThrowOrGet<ILogger>();
            _playerObservatory = playerObservatory ?? throw new ArgumentNullException(nameof(playerObservatory));

            LocalPlayer = player ?? throw new ArgumentNullException(nameof(player));

            PlayerSync = new PlayerUpdateSyncMachine(LocalPlayer);
            NpcSync = new NpcUpdateSyncMachine(LocalPlayer);

            LocalPlayer.Connection.SyncMachines.Add(PlayerSync);
            LocalPlayer.Connection.SyncMachines.Add(NpcSync);
        }

        public void Clear()
        {
            PlayerSync.Clear();
            NpcSync.Clear();
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
                            // todo : handle GroundItem in ObserverSyncMachine
                        default:
                            _log.Warning(this, $"Unhandled entity in isNew sync: {obs}");
                            break;
                    }
                }
            }
        }

        public void OnReinitialize()
        {
            _playerObservatory.ReevaluateSightOverride = true;
            _playerObservatory.Clear();
        }
    }
}
