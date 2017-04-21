using System;
using JetBrains.Annotations;

namespace cscape
{
    public abstract class SyncMachine
    {
        public GameServer Server { get; }

        public SyncMachine(GameServer server)
        {
            Server = server;
        }

        public abstract void Synchronize([NotNull] Blob stream);

        private bool _isWritingPacket;
        private int _payloadLengthIndex = -1;
        private bool _isShortLength;

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

            stream.Write(id);

            if (length >= 0)
                return;

            _isWritingPacket = true;
            stream.Write(0); // placeholder
            _payloadLengthIndex = stream.WriteCaret - 1;

            if (length == PacketLength.NextShort)
            {
                stream.Write(0); // placeholder
                _isShortLength = true;
            }
            else
                _isShortLength = false;

        }

        protected void EndPacket(Blob stream)
        {
            if (!_isWritingPacket) return;

            // figure out how big the payload is in bytes.
            var written = stream.WriteCaret - _payloadLengthIndex - (_isShortLength ? 2 : 1);

            // write it in place of the placeholder 0's
            if (_isShortLength)
            {
                stream.Buffer[_payloadLengthIndex] = (byte) (written >> 8);
                stream.Buffer[_payloadLengthIndex+1] = (byte)written;
            }
            else
                stream.Buffer[_payloadLengthIndex] = (byte) written;

            _isWritingPacket = false;
            _payloadLengthIndex = -1;
        }
    }
}