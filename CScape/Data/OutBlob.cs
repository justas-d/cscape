using System;
using CScape.Network;

namespace CScape.Data
{
    public class OutBlob : Blob
    {
        public GameServer Server { get; }

        public OutBlob(GameServer server, int size) : base(new byte[size])
        {
            Server = server;
        }

        private bool _isWritingPacket;
        private int _payloadLengthIndex = -1;
        private bool _isShortLength;

        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">Cannot begin writing a packet whose length is undefined or the encoded in the next two bytes.</exception>
        /// <exception cref="InvalidOperationException">Cannot begin writing packet when already writing a packet.</exception>
        public void BeginPacket(byte id)
        {
            if (_isWritingPacket)
                throw new InvalidOperationException("Cannot begin writing packet when already writing a packet.");

            var length = Server.Database.Packet.GetOutgoing(id);

            if (length == PacketLength.Undefined)
                throw new NotSupportedException("Cannot begin writing a packet whose length is undefined.");

            Write(id);

            if (length >= 0)
                return;

            _isWritingPacket = true;
            Write(0); // placeholder
            _payloadLengthIndex = WriteCaret - 1;

            if (length == PacketLength.NextShort)
            {
                Write(0); // placeholder
                _isShortLength = true;
            }
            else
                _isShortLength = false;

        }

        public void EndPacket()
        {
            if (!_isWritingPacket) return;

            // figure out how big the payload is in bytes.
            var written = WriteCaret - _payloadLengthIndex - (_isShortLength ? 2 : 1);

            // write it in place of the placeholder 0's
            if (_isShortLength)
            {
                Buffer[_payloadLengthIndex] = (byte)(written >> 8);
                Buffer[_payloadLengthIndex + 1] = (byte)written;
            }
            else
                Buffer[_payloadLengthIndex] = (byte)written;

            _isWritingPacket = false;
            _payloadLengthIndex = -1;
        }

        /// <summary>
        /// Writes a byte, if value is under 255. If value is equal to, or over, 255, writes it as an 255 padding and then the value as int32.
        /// </summary>
        public void WriteByteInt32Smart(int value)
        {
            if (value >= 255)
            {
                Write(255);
                Write32(value);
            }
            else
                Write((byte)value);
        }
    }
}