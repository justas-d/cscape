using System;
using CScape.Data;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network.Sync
{
    public sealed class InterfaceSyncMachine : SyncMachine
    {
        public Player Player { get; }
        public override int Order => SyncMachineConstants.Interface;

        public InterfaceSyncMachine([NotNull] Player player) : base(player.Server)
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
