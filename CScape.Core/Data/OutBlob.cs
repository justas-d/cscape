using System;
using CScape.Core.Network;

namespace CScape.Core.Data
{
    public class OutBlob : Blob
    {
        private readonly IPacketDatabase _db;

        public OutBlob(IServiceProvider service, int size) : base(new byte[size])
        {
            _db = service.ThrowOrGet<IPacketDatabase>();
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

            var length = _db.GetOutgoing(id);

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

        /// <summary>
        /// Ends writing the current packet but writes the given <see cref="sizeOverload"/> in place of the packet payload size.
        /// </summary>
        public void EndPacket(int sizeOverload)
        {
            if (!_isWritingPacket) return;

            // write it in place of the placeholder 0's
            if (_isShortLength)
            {
                Buffer[_payloadLengthIndex] = (byte)(sizeOverload >> 8);
                Buffer[_payloadLengthIndex + 1] = (byte)sizeOverload;
            }
            else
                Buffer[_payloadLengthIndex] = (byte)sizeOverload;

            _isWritingPacket = false;
            _payloadLengthIndex = -1;

        }

        public void EndPacket()
            // figure out how big the payload is in bytes.
            => EndPacket(WriteCaret - _payloadLengthIndex - (_isShortLength ? 2 : 1));

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