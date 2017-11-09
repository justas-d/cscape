using System.Collections.Generic;
using System.Linq;
using CScape.Core.Network.Entity.Segment;
using JetBrains.Annotations;

namespace CScape.Core.Network.Packet
{
    public static class GenericEntityUpdateWriter
    {
        public static void WriteIntoPacket(OutBlob stream,
            [NotNull] IEnumerable<IUpdateSegment> syncSegments,
            [NotNull] IEnumerable<IUpdateSegment> initializeSegments,
            [NotNull] IEnumerable<IUpdateSegment> flagSegments,
            int numIdBits, int maxIdValue)
        {
            stream.BeginBitAccess();

            WriteSyncSegments(stream, syncSegments);
            WriteInitSegments(stream, initializeSegments);

            // avoid double enumeration of the flagSegments enumerable
            var enumeratedUpdateSegments =
                flagSegments as IList<IUpdateSegment> ?? flagSegments.ToList();

            WriteInitSegmentsEndMarkerIfNeeded(stream, enumeratedUpdateSegments, numIdBits, maxIdValue);

            stream.EndBitAccess();

            WriteFlagSegments(stream, enumeratedUpdateSegments);
        }

        private static void WriteInitSegmentsEndMarkerIfNeeded(
            OutBlob stream, IList<IUpdateSegment> flagSegments,
            int numIdBits, int maxIdValue)
        {
            if (flagSegments.Any())
                stream.WriteBits(numIdBits, maxIdValue);
        }

        private static void WriteFlagSegments(OutBlob stream, IList<IUpdateSegment> flagSegments)
        {
            foreach (var flag in flagSegments)
                flag.Write(stream);
        }

        private static void WriteInitSegments(OutBlob stream, IEnumerable<IUpdateSegment> initializeSegments)
        {
            foreach (var init in initializeSegments)
                init.Write(stream);
        }

        private static void WriteSyncSegments(OutBlob stream, IEnumerable<IUpdateSegment> syncSegments)
        {
            // avoid double enumeration if the syncSegments IEnumerable
            var enumeratedSyncSegments =
                syncSegments as IList<IUpdateSegment> ?? syncSegments.ToList();

            stream.WriteBits(8, enumeratedSyncSegments.Count);

            foreach (var segment in enumeratedSyncSegments)
                segment.Write(stream);
        }
    }
}