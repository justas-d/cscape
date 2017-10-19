using System;
using CScape.Models.Game.Message;

namespace CScape.Models.Game.Interface
{
    /// <summary>
    /// Defines an interface which can be identified using an id and can receive game messages.
    /// </summary>
    public interface IGameInterface : IEquatable<IGameInterface>
    {
        /// <summary>
        /// The ID of the interface.
        /// </summary>
        int Id { get; }

        void ReceiveMessage(IGameMessage msg);
    }
}