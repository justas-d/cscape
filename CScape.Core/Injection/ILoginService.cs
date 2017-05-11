using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    /// <summary>
    /// Provides an interface to a player log-in service which handles players attempting to log in.
    /// </summary>
    public interface ILoginService 
    {
        /// <summary>
        /// Whether the login service is active or not.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Attempts to return the next queued login request.
        /// </summary>
        /// <returns>Null if there are no more loggin requests. If there are login requests return the return the next request as an <see cref="IPlayerLogin"/></returns>
        [CanBeNull] IPlayerLogin TryGetNext();
    }
}