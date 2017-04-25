using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using CScape.Game.Entity;
using CScape.Network.Sync;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CScape.Network
{
    public class SocketContext : IDisposable
    {
        public const int OutStreamSize = (byte.MaxValue + 1) * 128;
        public const int InBufferStreamSize = 1024 * 2;
        public const int InStreamSize = InBufferStreamSize * 3;
        //~78.125MB total mem used with 2000 concurrent players.

        /// <summary>
        /// In milliseconds, the delay between a socket dying and it's player being removed
        /// from the player entity pool. todo Default: 60 seconds.
        /// </summary>
        public long ReapTimeMs { get; } = 1000 * 10;

        /// <summary>
        /// In milliseconds, the maximum number of ms that can ellapse without receiving a packet.
        /// If this number is exceeded (by _msSinceNoPacket), the socket is considered dead.
        /// </summary>
        public const int MaxNoPacketIntervalMsg = 5 * 1000;

        private long _msSinceNoPacket;
        private long _deadForMs;

        [CanBeNull]
        public Socket Socket { get; private set; }

        [NotNull]
        public OutBlob OutStream { get; }

        [NotNull]
        public Blob InCircularStream { get; }

        private readonly byte[] _inBufferStream; // for buffering writes to InCircularStream

        [NotNull]
        public GameServer Server { get; }

        private Logger Log => Server.Log;

        [NotNull]
        public Player Player { get; }
        public int SignlinkId { get; }

        public bool IsDisposed { get; private set; }

        [NotNull] public List<SyncMachine> SyncMachines { get; } = new List<SyncMachine>();
        private readonly MessageSyncMachine _msgSync;

        /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/></exception>
        public SocketContext([NotNull] Player player, [NotNull] GameServer server, [NotNull] Socket socket, int signLink)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Socket.Blocking = false;

            OutStream = new OutBlob(server, OutStreamSize);
            InCircularStream = new Blob(InStreamSize, true);
            _inBufferStream = new byte[InBufferStreamSize];

            SignlinkId = signLink;

            _msgSync = new MessageSyncMachine(Server);
            SyncMachines.Add(_msgSync);
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
            if (IsDisposed)
                return;

            try
            {
                var avail = Socket.Available;
                if (avail <= 0) return;

                var recv = Socket.Receive(_inBufferStream, 0, avail, SocketFlags.None);

                InCircularStream.WriteBlock(_inBufferStream, 0, recv);
            }
            catch (Exception e) when (e is CircularBlobException || e is ArgumentOutOfRangeException)
            {
                Player.ForcedLogout();
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
            Dispose();
        }

        public bool IsConnected()
        {
            if (IsDisposed)
                return false;

            if (Socket == null)
                return false;

            if (_msSinceNoPacket >= MaxNoPacketIntervalMsg)
            {
                Dispose();
                return false;
            }

            return true;
        }

        public void UpdateLastPacketReceivedTime()
        {
            ThrowIfDisposed();
            _msSinceNoPacket = 0;
        }

        public void SendOutStream()
        {
            if (IsDisposed)
                return;

            // return if we're haven't actually written anything to the output blob
            if (OutStream.WriteCaret <= 0)
                return;

            try
            {
                Socket.Send(OutStream.Buffer, 0, OutStream.WriteCaret, SocketFlags.None);
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
        /// <param name="deltaTime">Milliseconds that have elapsed since the last call to this method.</param>
        /// <returns>Returns true when the owning player can be removed from the</returns>
        public bool ManageHardDisconnect(long deltaTime)
        {
            _msSinceNoPacket += deltaTime;

            if (IsConnected()) return false;

            _deadForMs = _deadForMs + deltaTime;
            return _deadForMs >= ReapTimeMs;
        }

        public void SortSyncMachines()
        {
            ThrowIfDisposed();
            SyncMachines.Sort((x, y) => x.Order.CompareTo(y.Order));
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException("SocketContext");
        }

        public void SendMessage(IPacket smsg)
            => _msgSync.Enqueue(smsg);

        public void Dispose()
        {
            if (IsDisposed)
                return;

            SyncMachines.Clear();
            Socket?.Dispose();
            Socket = null;
            IsDisposed = true;
        }
    }
}