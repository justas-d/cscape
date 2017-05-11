using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IGameServer : IDisposable
    {
        [NotNull] AggregateEntityPool<IWorldEntity> Entities { get; }
        [NotNull] IReadOnlyDictionary<int, Player> Players { get; }

        [NotNull] IServiceProvider Services { get; }
        [NotNull] PlaneOfExistance Overworld { get; }

        bool IsDisposed { get; }
        DateTime StartTime { get; }

        ServerStateFlags GetState();
        [NotNull] Task Start();
        [CanBeNull] Player GetPlayerByPid(int pid);

        void RegisterPlayer([NotNull] Player player);
        void UnregisterPlayer([NotNull] Player player);
    }
}