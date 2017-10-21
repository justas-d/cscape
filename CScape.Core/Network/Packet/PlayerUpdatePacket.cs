using System.Collections.Generic;
using System.Linq;
using CScape.Core.Network.Entity.Segment;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Packet
{
    public sealed class PlayerUpdatePacket : IPacket
    {
        [NotNull] private readonly IUpdateSegment _localSegment;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _syncSegments;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _initializeSegments;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _flagSegments;

        public const int Id = 81;

        public PlayerUpdatePacket(
            [NotNull] IUpdateSegment localSegment,
            [NotNull] IEnumerable<IUpdateSegment> syncSegments,
            [NotNull] IEnumerable<IUpdateSegment> initializeSegments,
            [NotNull] IEnumerable<IUpdateSegment> flagSegments)
        {
            _localSegment = localSegment;
            _syncSegments = syncSegments;
            _initializeSegments = initializeSegments;
            _flagSegments = flagSegments;
        }

        public void Send(OutBlob stream)
        { 
            stream.BeginPacket(Id);

            stream.BeginBitAccess();

            _localSegment.Write(stream);

            stream.WriteBits(8, _syncSegments.Count());

            foreach (var segment in _syncSegments)
                segment.Write(stream);

            foreach(var init in _initializeSegments)
                init.Write(stream);

            if (_flagSegments.Any())
            {
                stream.WriteBits(11, 2047);
                stream.EndBitAccess();

                foreach (var flag in _flagSegments)
                    flag.Write(stream);
            }

            stream.EndPacket();
        }
    }
}