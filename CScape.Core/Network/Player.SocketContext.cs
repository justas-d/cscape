using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using CScape.Core.Data;
using CScape.Core.Injection;
using CScape.Core.Network.Sync;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public class SocketContext : ISocketContext
    {
        private sealed class SyncMachineCollection : IList<ISyncMachine>
        {
            private readonly SortedDictionary<int, ISyncMachine> _sync
                = new SortedDictionary<int, ISyncMachine>();

            public int Count => _sync.Count;
            public bool IsReadOnly => false;

            public void Add([NotNull] ISyncMachine item)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));

                // try adding, guaranteeing order
                if (_sync.ContainsKey(item.Order))
                    throw new InvalidOperationException();

                _sync.Add(item.Order, item);
            }

            public IEnumerator<ISyncMachine> GetEnumerator() => _sync.Values.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Clear() => _sync.Clear();
            public bool Contains(ISyncMachine item) => _sync.ContainsKey(item.Order);

            public void CopyTo(ISyncMachine[] array, int arrayIndex) => throw new NotImplementedException();
            public void Insert(int index, ISyncMachine item) => throw new NotImplementedException();

            public bool Remove(ISyncMachine item) => _sync.Remove(item.Order);
            public void RemoveAt(int index) => _sync.Remove(index);

            public int IndexOf(ISyncMachine item)
            {
                if (!_sync.ContainsKey(item.Order))
                    return -1;
                return item.Order;
            }


            public ISyncMachine this[int index]
            {
                get => _sync[index];
                set => throw new NotImplementedException();
            }
        }

        public const int OutStreamSize = 5000;
        public const int InBufferStreamSize = 1024;
        public const int InStreamSize = InBufferStreamSize * 2;
        //~15.40MB total mem used with 2000 concurrent players.

        /// <summary>
        /// In milliseconds, the maximum number of ms that can ellapse without receiving a packet.
        /// If this number is exceeded (by _msSinceNoPacket), the socket is considered dead.
        /// </summary>
        public const int MaxNoDataInternal = 5 * 1000;

        private long _msSinceData; // how many ms have passed since we've received data.


        public OutBlob OutStream { get; }
        public CircularBlob InStream { get; private set; }

        public IList<ISyncMachine> SyncMachines { get; } = new SyncMachineCollection();

        public long DeadForMs { get; private set; }
        public int SignlinkId { get; private set; }
        public bool IsDisposed { get; private set; }

        private readonly byte[] _inBufferStream;
        private Socket _socket; // only null when IsDisposed
        private readonly ILogger _log;

        private MessageSyncMachine _msg;

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

            SyncMachines.Add(new MessageSyncMachine());
            _msg = (MessageSyncMachine)SyncMachines[SyncMachineConstants.Message];
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

            // notify sync machines
            for (var i = 0; i < SyncMachines.Count; i++)
            {
                var cur = SyncMachines[i];
                SyncMachines[i].OnReinitialize();

                if(cur.RemoveAfterInitialize)
                {
                    // remove
                    SyncMachines.RemoveAt(i);
                    --i;
                }
            }

            _msg = (MessageSyncMachine)SyncMachines[SyncMachineConstants.Message];

            return true;
        }

        public bool Update(long deltaTime)
        {
            // check if we're still alive.
            _msSinceData += deltaTime;

            if (!IsConnected())
            {
                // paranoid about an edge-case where DeadForMs does overflow due to zombie players.
                unchecked { DeadForMs += deltaTime; }
                return false;
            }

            // flush input data
            try
            {
                var avail = _socket.Available;
                if (avail <= 0) return true; // no data received, all is good.

                var recv = _socket.Receive(_inBufferStream, 0, avail, SocketFlags.None);

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
                return true; // succesfully, do nothing

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

        public void SendPacket(IPacket packet) => _msg.Enqueue(packet);

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