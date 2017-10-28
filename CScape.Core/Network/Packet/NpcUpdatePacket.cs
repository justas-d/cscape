using System.Collections.Generic;
using System.Linq;
using CScape.Core.Network.Entity.Segment;
using CScape.Models.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Packet
{
    public sealed class NpcUpdatePacket : IPacket
    {
        [NotNull] private readonly IEnumerable<IUpdateSegment> _syncSegments;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _initializeSegments;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _flagSegments;

        public const int Id = 65;

        public NpcUpdatePacket(
            [NotNull] IEnumerable<IUpdateSegment> syncSegments,
            [NotNull] IEnumerable<IUpdateSegment> initializeSegments,
            [NotNull] IEnumerable<IUpdateSegment> flagSegments)
        {
            _syncSegments = syncSegments;
            _initializeSegments = initializeSegments;
            _flagSegments = flagSegments;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.BeginBitAccess();

            stream.WriteBits(8, _syncSegments.Count());

            foreach (var segment in _syncSegments)
                segment.Write(stream);

            foreach (var init in _initializeSegments)
                init.Write(stream);

            if (_flagSegments.Any())
            {
                stream.WriteBits(14, 16383);
                stream.EndBitAccess();

                foreach (var flag in _flagSegments)
                    flag.Write(stream);
            }
            else
                stream.EndBitAccess();

            stream.EndPacket();
        }
    }
}