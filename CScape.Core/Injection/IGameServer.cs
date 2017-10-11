using System;
using System.Threading.Tasks;
using CScape.Core.Game.Entities;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IGameServer : IDisposable
    {
        [NotNull] IServiceProvider Services { get; }
        [NotNull] PlaneOfExistence Overworld { get; }

        IEntitySystem Entities { get; }

        bool IsDisposed { get; }
        DateTime StartTime { get; }

        ServerStateFlags GetState();
        [NotNull] Task Start();
    }
}