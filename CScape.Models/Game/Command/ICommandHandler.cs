namespace CScape.Models.Game.Command
{
    public interface ICommandHandler
    {
        /// <summary>
        /// Tries to find and dispatch a command matching the given input and callee.
        /// </summary>
        /// <returns>True if command to dispatch was found, false if no command was found.</returns>
        bool Push([NotNull] Entity callee, [NotNull] string input);

    }
}
