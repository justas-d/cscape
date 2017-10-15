using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IMainLoop
    {
        IGameServer Server { get; }

        /// <summary>
        /// In milliseconds, how many ms it took to process the previous tick.
        /// </summary>
        long TickProcessTime { get; }

        /// <summary>
        /// In milliseconds, how long a tick should be.
        /// </summary>
        int TickRate { get; set; }

        bool IsRunning { get; }

        /// <summary>
        /// Get the change in time between the start of this tick and the call to this method.
        /// </summary>
        long GetDeltaTime();

        [NotNull] Task Run();
    }
}