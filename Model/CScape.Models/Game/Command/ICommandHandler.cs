using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Models.Game.Command
{
    /// <summary>
    /// Defines an entiry point to a command system.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Tries to find and dispatch a command matching the given input and callee.
        /// </summary>
        /// <returns>True if command to dispatch was found, false if no command was found.</returns>
        bool Push([NotNull] IEntity callee, [NotNull] string input);
    }
}
