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

        public const int NumIdBits = 14;
        public const int MaxIdValue = 16383;
        public const int PacketId = 65;

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
            stream.BeginPacket(PacketId);
            stream.BeginBitAccess();

            GenericEntityUpdateWriter.WriteIntoPacket(stream,
                _syncSegments,
                _initializeSegments,
                _flagSegments,
                NumIdBits,
                MaxIdValue);

            stream.EndPacket();
        }
    }
}