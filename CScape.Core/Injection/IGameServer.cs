using System;
using System.Threading.Tasks;
using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IGameServer : IDisposable
    {
        [NotNull] AggregateEntityPool<IWorldEntity> Entities { get; }
        [NotNull] IEntityRegistry<short, Player> Players { get; }
        [NotNull] IEntityRegistry<int, Npc> Npcs { get; }

        [NotNull] IServiceProvider Services { get; }
        [NotNull] PlaneOfExistance Overworld { get; }

        bool IsDisposed { get; }
        DateTime StartTime { get; }

        ServerStateFlags GetState();
        [NotNull] Task Start();
    }
}