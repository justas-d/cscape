using System;
using System.Collections.Generic;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace cscape
{
    public sealed partial class Player : Entity
    {
        public class SocketContext
        {
            public const int OutStreamSize = (byte.MaxValue + 1) * 128; 
            public const int InBufferStreamSize = 1024 * 2;
            public const int InStreamSize = InBufferStreamSize * 3;
            //~78.125MB total mem used with 2000 concurrent players.

            // todo : increase poll time if socket is experiencing disconnects?
            public int PollTime { get; } = 1000;

            /// <summary>
            /// In milliseconds, the delay between a socket dying and it's player being removed
            /// from the player entity pool. todo Default: 60 seconds.
            /// </summary>
            public long ReapTimeMs { get; } = 1000 * 2;
            private long _deadForMs = 0;

            [CanBeNull]
            public Socket Socket { get; private set; }

            [NotNull]
            public Blob OutStream { get; }

            [NotNull]
            public Blob InCircularStream { get; }
            private readonly byte[] _inBufferStream; // for buffering writes to InCircularStream

            [NotNull]
            public GameServer Server { get; }

            private Logger Log => Server.Log;

            public int SignlinkId { get; }

            [NotNull]
            public readonly List<SyncMachine> SyncMachines = new List<SyncMachine>();

            /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/></exception>
            public SocketContext([NotNull] GameServer server, [NotNull] Socket socket, int signLink)
            {
                Socket = socket ?? throw new ArgumentNullException(nameof(socket));
                Socket.Blocking = false;

                OutStream = new Blob(OutStreamSize);
                InCircularStream = new Blob(InStreamSize, true);
                _inBufferStream = new byte[InBufferStreamSize];

                Server = server;
                SignlinkId = signLink;
            }

            /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/></exception>
            public void AssignNewSocket([NotNull] Socket socket)
            {
                Socket = socket ?? throw new ArgumentNullException(nameof(socket));
                _deadForMs = 0;
            }

            /// <summary>
            /// Flushes all passively buffered data received into the circular input stream.
            /// Socket must be connected.
            /// </summary>
            public void FlushInput()
            {
                try
                {
                    var avail = Socket.Available;
                    if (avail <= 0) return;

                    var recv = Socket.Receive(_inBufferStream, 0, avail, SocketFlags.None);

                    InCircularStream.WriteBlock(_inBufferStream, 0, recv);
                }
                catch (Exception e) when ( e is CircularBlobException || e is ArgumentOutOfRangeException)
                {
                    // todo : drop the player FlushInput fails.
                    Log.Warning(this, $"Exception on flush: {e.Message}.");
                }
                catch (Exception e) when (e is SocketException || e is ObjectDisposedException)
                {
                    HandleSocketOrDisposedException(e);
                }
            }

            private void HandleSocketOrDisposedException(Exception e)
            {
                Log.Debug(this, $"Socket or Disposed exception: {e.Message}");
                Socket?.Dispose();
                Socket = null;
            }

            public bool IsConnected()
            {
                if (Socket == null) return false;

                var part1 = Socket.Poll(PollTime, SelectMode.SelectRead);
                var part2 = Socket.Available == 0;
                if (part1 && part2)
                    return false;
                return true;
            }

            public void SendOutStream()
            {
                // return if we're haven't actually written anything to the output blob
                if (OutStream.WriteCaret <= 0) return;

                try
                {
                    Socket?.Send(OutStream.Buffer, 0, OutStream.WriteCaret, SocketFlags.None);
                }
                catch (Exception e) when (e is SocketException || e is ObjectDisposedException)
                {
                    HandleSocketOrDisposedException(e);
                }
                finally
                {
                    OutStream.ResetWrite();
                }
            }

            /// <summary>
            /// Checks for "hard disconnects" (force closes of the client/socket).
            /// Manages the state of the socket while in one. 
            /// </summary>
            /// <param name="time">Milliseconds that have elapsed since the last call to this method.</param>
            /// <returns>Returns true when the owning player can be removed from the</returns>
            public bool ManageHardDisconnect(long time)
            {
                if (IsConnected()) return false;

                Socket = null;
                _deadForMs = _deadForMs + time;
                return _deadForMs >= ReapTimeMs;
            }
        }

    }
}