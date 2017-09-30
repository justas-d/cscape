using System;
using System.Net.Sockets;
using CScape.Core.Data;
using CScape.Core.Network;

namespace CScape.Core.Injection
{

    public interface ISocketContext : IDisposable
    {
        /// <summary>
        /// The data that will be sent to the client.
        /// </summary>
        OutBlob OutStream { get; }

        /// <summary>
        /// The circular stream of data sent to us from the client.
        /// </summary>
        CircularBlob InStream { get; }

        /// <summary>
        /// The client's signlink id.
        /// </summary>
        int SignlinkId { get; }

        /// <summary>
        /// How long, in milliseconds, the socket has been dead for.
        /// </summary>
        long DeadForMs { get; }

        /// <summary>
        /// Whether this context is disposed or not.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Attempts to reinitialize the context around the given socket.
        /// </summary>
        /// <param name="socket">The two-way stream socket to the client.</param>
        /// <param name="signlinkId">The signlink id of the client.</param>
        /// <returns>True on successful reinitialization, false otherwise.</returns>
        bool TryReinitialize(Socket socket, int signlinkId);

        bool CanReinitialize(int signlinkId);

        /// <summary>
        /// Called every tick to update and run arbitrary proceedures on the connection.
        /// </summary>
        /// <param name="deltaTime">The time between the call to this update and the last game tick.</param>
        /// <returns>True on succesful update, false otherwise.</returns>
        bool Update(long deltaTime);

        /// <summary>
        /// Flushes -- sends -- the data in the output stream to the client socket.
        /// </summary>
        /// <returns>True if flush was succesful, false otherwise.</returns>
        bool FlushOutputStream();

        /// <summary>
        /// Returns whether the client is connected, true, or not, false.
        /// </summary>
        bool IsConnected();

        /// <summary>
        /// Schedules a packet to be sent to the client.
        /// </summary>
        void SendPacket(IPacket packet);
    }
}
