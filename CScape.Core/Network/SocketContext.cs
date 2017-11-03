using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using CScape.Core.Extensions;
using CScape.Models;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public class SocketContext : ISocketContext
    {
        public const int OutStreamSize = 5000;
        public const int InBufferStreamSize = 1024;
        public const int InStreamSize = InBufferStreamSize * 2;
        //~15.4MB total mem used with 2000 concurrent players.

        /// <summary>
        /// In milliseconds, the maximum number of ms that can elapse without receiving a packet.
        /// If this number is exceeded (by _msSinceNoPacket), the socket is considered dead.
        /// </summary>
        public const int MaxNoDataInternal = 5 * 1000;

        private long _msSinceData; // how many ms have passed since we've received data.

        public OutBlob OutStream { get; }
        public CircularBlob InStream { get; private set; }

        public long DeadForMs { get; private set; }
        public int SignlinkId { get; private set; }
        public bool IsDisposed { get; private set; }

        private readonly byte[] _inBufferStream;
        private Socket _socket; // only null when IsDisposed
        private readonly ILogger _log;

        public SocketContext([NotNull] IServiceProvider services,
            [NotNull] Socket socket, int signlinkId)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
            _log = services.ThrowOrGet<ILogger>();

            SignlinkId = signlinkId;

            OutStream = new OutBlob(services, OutStreamSize);
            InStream = new CircularBlob(InStreamSize);

            _inBufferStream = new byte[InBufferStreamSize];
            _socket.Blocking = false;
        }

        public bool CanReinitialize(int signlinkId) => signlinkId == SignlinkId && !IsConnected();

        public bool TryReinitialize([NotNull] Socket socket, int signlinkId)
        {
            if (!socket.Connected)
                return false;

            if (!CanReinitialize(signlinkId))
                return false;

            DeadForMs = 0;
            _msSinceData = 0;

            IsDisposed = false;
            _socket = socket;

            // reset streams
            OutStream.ResetHeads();
            InStream = new CircularBlob(InStreamSize);

            return true;
        }

        public bool Update(long deltaTime)
        {
            // check if we're still alive.
            _msSinceData += deltaTime;

            // flush input data
            try
            {
                if (!IsConnected())
                {
                    DeadForMs += deltaTime;
                    return false;
                }

                var avail = _socket.Available;

                if (avail <= 0)
                    return true; // no data received, all is good.

                // flush the stuff we received into _inBufferStream
                var recv = _socket.Receive(_inBufferStream, 0, avail, SocketFlags.None);

                // flush the _inBufferStream into the circular InStream
                InStream.WriteBlock(_inBufferStream, 0, recv);
                _msSinceData = 0;

                return true;
            }
            catch (Exception e)
            {
                HandleException(e);
            }

            return false;
        }

        public bool FlushOutputStream()
        {
            if (!IsConnected())
                return false;

            // return if we're haven't actually written anything to the output blob
            if (OutStream.WriteCaret <= 0)
                return true; // successfully, do nothing

            // flush output data
            try
            {
                _socket.Send(OutStream.Buffer, 0, OutStream.WriteCaret, SocketFlags.None);
                OutStream.ResetWrite();
                return true;
            }
            catch (Exception e)
            {
                HandleException(e);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HandleException(Exception e)
        {
            _log.Debug(this, $"Socket context exception: {e.Message}");
            Dispose();
        }
 
        public bool IsConnected()
        {
            if (IsDisposed)
                return false;

            if (_msSinceData >= MaxNoDataInternal)
            {
                Dispose();
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            try
            {
                _socket.Dispose();
            }
            catch (Exception ex)
            {
                _log.Exception(this, "Failed to dispose socket.", ex);
            }

            _socket = null;
        }
    }
}