using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CScape.Models
{
    public interface IMainLoop
    {
        /// <summary>
        /// The parent server.
        /// </summary>
        [NotNull]
        IGameServer Server { get; }

        /// <summary>
        /// In milliseconds, how many ms it took to process the previous tick.
        /// </summary>
        long TickProcessTime { get; }

        /// <summary>
        /// In milliseconds, how long a tick should be.
        /// </summary>
        int TickRate { get; set; }

        /// <summary>
        /// Gets or sets whether this main loop is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Get the change in time between the start of this tick and the call to this method.
        /// </summary>
        long GetDeltaTime();

        /// <summary>
        /// Returns a task for the infinite tick loop proceedure.
        /// </summary>
        [NotNull] Task Run();
    }
}