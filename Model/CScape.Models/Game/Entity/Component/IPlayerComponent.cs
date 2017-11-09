using System;

namespace CScape.Models.Game.Entity.Component
{
    public interface IPlayerComponent : IEquatable<IPlayerComponent>, IEquatable<string>, IEntityComponent
    {
        /// <summary>
        /// The instance id of the player.
        /// </summary>
        int InstanceId { get; }

        /// <summary>
        /// The implementation specific id of the player's title.
        /// </summary>
        int TitleId { get; set; }

        /// <summary>
        /// The player's username.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Forcefully logs the player out.
        /// </summary>
        void ForcedLogout();

        /// <summary>
        /// Attempts to log the player out.
        /// </summary>
        /// <returns>True if the player was logged out succesfully, false otherwise.</returns>
        bool TryLogout();
    }
}