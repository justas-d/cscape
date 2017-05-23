using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Core.Data;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// Encodes a wrapper around many <see cref="BaseGroundObjectPacket"/> packets, prefacing them
    /// with the X and Y values of the active region for updating.
    /// </summary>
    public class EmbeddedRegionGroundObjectWrapperPacket : IPacket
    {
        private readonly byte _playerLocalX;
        private readonly byte _playerLocalY;

        private readonly IEnumerable<BaseGroundObjectPacket> _embedded;

        public const int Id = 60;

        public EmbeddedRegionGroundObjectWrapperPacket(
            (int x, int y) local,
            IEnumerable<BaseGroundObjectPacket> embedded)
        {
            _playerLocalX = Convert.ToByte(local.x);
            _playerLocalY = Convert.ToByte(local.y);
            _embedded = embedded;
        }

        public EmbeddedRegionGroundObjectWrapperPacket(
            (int x, int y) local,
            params BaseGroundObjectPacket[] embedded)
        {
            _playerLocalX = Convert.ToByte(local.x);
            _playerLocalY = Convert.ToByte(local.y);
            _embedded = embedded;
        }

        public void Send(OutBlob stream)
        {
            if (!_embedded.Any()) return;

            stream.BeginPacket(Id);

            stream.Write(_playerLocalY);
            stream.Write(_playerLocalX);

            foreach (var embedded in _embedded.Where(p => !p.IsInvalid))
                embedded.Send(stream);

            stream.EndPacket();
        }
    }
}