using System.Threading.Tasks;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IUpdateBatch
    {
        [NotNull] IUpdateQueue<IMovingEntity> Movement { get; }
        [NotNull] IUpdateQueue<Player> Player { get; }
        [NotNull] IUpdateQueue<Npc> Npc { get; }
        [NotNull] IUpdateQueue<GroundItem> Item { get; }
    }

    public interface IMainLoop
    {
        IUpdateBatch UpdHighFrequency { get; }
        IUpdateBatch UpdLowFrequency{ get; }

        long ElapsedMilliseconds { get; }
        long DeltaTime { get; }
        long TickProcessTime { get; }
        int TickRate { get; set; }

        bool IsRunning { get; }

        [NotNull] Task Run();
    }
}