using JetBrains.Annotations;

namespace CScape.Core.Game.Interface
{
    /// <summary>
    /// Defines an interface that can talk to the interface management system.
    /// </summary>
    public interface IApiInterface : IBaseInterface
    {
        /// <summary>
        /// Whether the api interface is registered and active in the api backend.
        /// </summary>
        bool IsRegistered { get; }

        bool TryRegisterApi([NotNull] IInterfaceManagerApiBackend api);
        void UnregisterApi();
    }
}