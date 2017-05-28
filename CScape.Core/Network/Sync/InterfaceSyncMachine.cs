using System;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    public sealed class InterfaceSyncMachine : ISyncMachine
    {
        public Player Player { get; }
        public int Order => SyncMachineConstants.Interface;
        public bool RemoveAfterInitialize { get; } = false;
        public bool NeedsUpdate => true;

        public InterfaceSyncMachine([NotNull] Player player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public void Synchronize(OutBlob stream)
        {
            foreach (var p in Player.Interfaces.GetUpdates())
                p.Send(stream);
        }

        public void OnReinitialize()
        {
        }
    }
}
