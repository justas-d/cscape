using System;
using System.Collections.Generic;
using CScape.Core.Network.Entity.Segment;
using JetBrains.Annotations;

namespace CScape.Core.Network.Packet
{
    public sealed class PlayerUpdatePacket : IPacket
    {
        [NotNull] private readonly IUpdateSegment _localSegment;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _syncSegments;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _initializeSegments;
        [NotNull] private readonly IEnumerable<IUpdateSegment> _flagSegments;

        public const int NumIdBits = 11;
        public const int MaxIdValue = 2047;
        public const int PacketId = 81;

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
            stream.BeginPacket(PacketId);
            stream.BeginBitAccess();

            _localSegment.Write(stream);

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