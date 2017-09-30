using System.Threading.Tasks;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IMainLoop
    {
        IGameServer Server { get; }

        long ElapsedMilliseconds { get; }
        long DeltaTime { get; }
        long TickProcessTime { get; }
        int TickRate { get; set; }

        bool IsRunning { get; }

        [NotNull] Task Run();
    }
}