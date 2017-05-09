using JetBrains.Annotations;

namespace CScape.Game.Interface
{
    /// <summary>
    /// Defines an interface which can be shown, therefore closed and exposed via the interface manager.
    /// </summary>
    public interface IShowableInterface : IApiInterface
    {
        /// <summary>
        /// Attempts to close and unregister this interface.
        /// </summary>
        bool TryClose();
        void Show();

        [CanBeNull] IButtonHandler ButtonHandler { get; }
    }
}