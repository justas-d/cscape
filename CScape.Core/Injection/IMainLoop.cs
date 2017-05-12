using System.Threading.Tasks;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IMainLoop
    {
        [NotNull] IUpdateQueue<IMovingEntity> Movement { get; }
        [NotNull] IUpdateQueue<Player> Player { get; }

        long ElapsedMilliseconds { get; }
        long DeltaTime { get; }
        long TickProcessTime { get; }
        int TickRate { get; set; }

        bool IsRunning { get; }

        [NotNull] Task Run();
    }
}