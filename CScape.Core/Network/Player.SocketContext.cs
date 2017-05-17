using System;
using System.Collections.Generic;
using System.Net.Sockets;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public class SocketContext : IDisposable
    {
        public const int OutStreamSize = 5000;
        public const int InBufferStreamSize = 1024;
        public const int InStreamSize = InBufferStreamSize * 2;
        //~15.40MB total mem used with 2000 concurrent players.

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
        public CircularBlob InCircularStream { get; private set; }

        private readonly byte[] _inBufferStream; // for buffering writes to InCircularStream
        private readonly ILogger _log;

        [NotNull]
        public Player Player { get; }
        public int SignlinkId { get; }

        public bool IsDisposed { get; private set; }

        [NotNull] public List<ISyncMachine> SyncMachines { get; } = new List<ISyncMachine>();

        private readonly MessageSyncMachine _msgSync;

        /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/></exception>
        public SocketContext([NotNull] IServiceProvider services, [NotNull] Player player, 
            [NotNull] Socket socket, int signLink)
        {
            _log = services.ThrowOrGet<ILogger>();
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            Player = player ?? throw new ArgumentNullException(nameof(player));

            OutStream = new OutBlob(services, OutStreamSize);
            InCircularStream = new CircularBlob(InStreamSize);
            _inBufferStream = new byte[InBufferStreamSize];

            Socket.Blocking = false;
            SignlinkId = signLink;

            SyncMachines.Add(_msgSync = new MessageSyncMachine());
        }

        /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/></exception>
        public void Reconnect([NotNull] Socket socket, int signlink)
        {
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));

            if (!CanReconnect(signlink))
                return;

            _deadForMs = 0;
            IsDisposed = false;

            // reset streams
            OutStream.ResetHeads();
            InCircularStream = new CircularBlob(InStreamSize);
        }

        public bool CanReconnect(int signlink)
            => signlink == SignlinkId && !IsConnected();

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
                _log.Warning(this, $"Exception on flush: {e.Message}.");
            }
            catch (Exception e) when (e is SocketException || e is ObjectDisposedException)
            {
                HandleSocketOrDisposedException(e);
            }
        }

        private void HandleSocketOrDisposedException(Exception e)
        {
            _log.Debug(this, $"Socket or Disposed exception: {e.Message}");
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

        public void UpdateLastPacketReceivedTime() => _msSinceNoPacket = 0;

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

            Socket?.Dispose();
            Socket = null;
            IsDisposed = true;
        }
    }
}