using System;
using System.Threading.Tasks;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IGameServer : IDisposable
    {
        /*
        [NotNull] IEntityRegistry<short, Player> Players { get; }
        [NotNull] IEntityRegistry<int, Npc> Npcs { get; }
        */

        [NotNull] IServiceProvider Services { get; }
        [NotNull] PlaneOfExistence Overworld { get; }

        bool IsDisposed { get; }
        DateTime StartTime { get; }

        ServerStateFlags GetState();
        [NotNull] Task Start();
    }
}