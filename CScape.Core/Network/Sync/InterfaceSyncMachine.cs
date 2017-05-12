using System;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Sync
{
    public sealed class InterfaceSyncMachine : SyncMachine
    {
        public Player Player { get; }
        public override int Order => SyncMachineConstants.Interface;

        public InterfaceSyncMachine([NotNull] Player player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public override void Synchronize(OutBlob stream)
        {
            foreach (var p in Player.Interfaces.GetUpdates())
                p.Send(stream);
        }
    }
}
