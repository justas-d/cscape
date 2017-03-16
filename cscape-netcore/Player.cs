using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace cscape
{
    public sealed class Player : Entity
    {
        public class SocketContext
        {
            public const int OutStreamSize = byte.MaxValue + 1 * 128; //~62.5MB total mem used with 2000 concurrent players.

            // todo : increase poll time if socket is experiencing disconnects?
            public int PollTime { get; } = 1000;

            /// <summary>
            /// In milliseconds, the delay between a socket dying and it's player being removed
            /// from the player entity pool. Default: 60 seconds.
            /// </summary>
            public long ReapTimeMs { get; } = 1000 * 60;
            private long _deadForMs = 0;

            [CanBeNull]
            public Socket Socket { get; private set; }

            [NotNull]
            public Blob OutStream { get; }

            /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/></exception>
            public SocketContext([NotNull] Socket socket)
            {
                if (socket == null) throw new ArgumentNullException(nameof(socket));
                Socket = socket;
                OutStream = new Blob(OutStreamSize);
            }

            /// <exception cref="ArgumentNullException"><paramref name="socket"/> is <see langword="null"/></exception>
            public void AssignNewSocket([NotNull] Socket socket)
            {
                if (socket == null) throw new ArgumentNullException(nameof(socket));

                Socket = socket;
                _deadForMs = 0;
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

        public const int MaxUsernameChars = 12;
        public const int MaxPasswordChars = 128;

        public int Id { get; }
        //todo: change username feature

        [NotNull]
        public string Username { get; private set; }
        //todo: change password feature
        [NotNull]
        public string PasswordHash { get; private set; }

        public byte TitleIcon { get; set; }

        [NotNull]
        public SocketContext Connection { get; }
        public int SignlinkId { get; }

        [NotNull]
        public GameServer Server { get; }

        [NotNull]
        public readonly List<SyncMachine> SyncMachines = new List<SyncMachine>();

        /// <exception cref="ArgumentNullException"><paramref name="login"/> is <see langword="null"/></exception>
        public Player([NotNull] NormalPlayerLogin login)
        {
            if (login == null) throw new ArgumentNullException(nameof(login));
            Connection = new SocketContext(login.Connection);
            SignlinkId = login.SignlinkUid;

            Id = login.Data.Id;
            Username = login.Data.Username;
            PasswordHash = login.Data.PasswordHash;
            TitleIcon = login.Data.TitleIcon;
            Server = login.Server;

            SyncMachines.Add(new RegionSyncMachine(Server, Position));
        }
    }

    public abstract class SyncMachine
    {
        public GameServer Server { get; }

        public SyncMachine(GameServer server)
        {
            Server = server;
        }

        public abstract void Synchronize([NotNull] Blob stream);

        private bool _isWritingPacket;
        private int _payloadLengthPos = -1;

        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">Cannot begin writing a packet whose length is undefined or the encoded in the next two bytes.</exception>
        /// <exception cref="InvalidOperationException">Cannot begin writing packet when already writing a packet.</exception>
        protected void BeginPacket([NotNull] Blob stream, byte id)
        {
            if(_isWritingPacket)
                throw new InvalidOperationException("Cannot begin writing packet when already writing a packet.");

            if (stream == null) throw new ArgumentNullException(nameof(stream));
            var length = Server.Database.Packet.GetOutgoing(id);

            if(length ==  PacketLength.Undefined)
                throw new NotSupportedException("Cannot begin writing a packet whose length is undefined.");
            if (length == PacketLength.NextShort)
                throw new NotSupportedException("Cannot begin writing a packet whose length is the next short.");
            
            stream.Write(id);
            if (length > 0)
            {
                stream.Write((byte)length);
                return;
            }

            switch (length)
            {
                case PacketLength.Variable:
                    break;
                case PacketLength.NextByte:
                    _isWritingPacket = true;
                    _payloadLengthPos = stream.BytesWritten;
                    stream.Write(0); // placeholder
                    break;
                default:
                    Debug.Fail("length fell through to default");
                    break;
            }
        }

        /// <exception cref="NotSupportedException">"Wrote too many bytes into one packet (max 255).</exception>
        protected void EndPacket(Blob stream)
        {
            if (!_isWritingPacket) return;

            var written = stream.BytesWritten - _payloadLengthPos;
            if (written > byte.MaxValue)
                throw new NotSupportedException($"Wrote too many bytes into one packet. {written} > {byte.MaxValue}");

            stream.Buffer[_payloadLengthPos] = (byte) written;
            _isWritingPacket = false;
            _payloadLengthPos = -1;

        }
    }

    public class RegionSyncMachine : SyncMachine
    {
        private readonly PositionController _pos;
        private Region _oldRegion;

        public const int RegionInitOpcode = 73;

        public RegionSyncMachine(GameServer server, PositionController pos) : base(server)
        {
            _pos = pos;
        }

        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/></exception>
        public override void Synchronize(Blob stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // send region init if regions changed
            if (!_oldRegion.Equals(_pos.CurrentRegion))
            {
                BeginPacket(stream, RegionInitOpcode);
                stream.Write16((short)_oldRegion.X);
                stream.Write16((short)_oldRegion.Y);
            }

            _oldRegion = _pos.CurrentRegion;
        }
    }

    public class PositionController
    {
        public int GlobalX { get; private set; }
        public int GlobalY { get; private set; }
        public int GlobalZ { get; private set; }

        public int LocalX { get; private set; }
        public int LocalY { get; private set; }

        public Region CurrentRegion { get; private set; }

        public bool IsRunning { get; set; }
    }

    public class World
    {
        // don't want to deal with tuples
        // { x, {y, region}}
        private readonly Dictionary<int, Dictionary<int, Region>> _regions = new Dictionary<int, Dictionary<int, Region>>();

        public Region GetRegion(ushort x, ushort y)
        {
            if(!_regions.ContainsKey(x))
                _regions[x] = new Dictionary<int, Region>();

            var yDict = _regions[x];
            if(!yDict.ContainsKey(y))
                yDict[y] = new Region(x,y);

            return yDict[y];
        }
    }

    public class Region : IEquatable<Region>
    {
        public Region(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }

        public ushort X { get; }
        public ushort Y { get; }

        public bool Equals(Region other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Region) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
    }
}