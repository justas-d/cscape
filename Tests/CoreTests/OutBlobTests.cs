using CScape.Core.Network;
using CScape.Dev.Tests.Impl;
using CScape.Models.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.External
{
    [TestClass]
    public class OutBlobTests
    {
        private class MockPacketDb : IPacketDatabase
        {
            public static readonly (byte opcode, byte size) ConstantSize = (1, 5);
            public const int NoSize = 2;
            public const int ByteSize = 3;
            public const int ShortSize = 4;
            public const int Undefined = 5;

            public PacketLength GetIncoming(byte id) => throw new System.NotImplementedException();

            public PacketLength GetOutgoing(byte id)
            {
                if (id == ConstantSize.opcode)
                    return (PacketLength) ConstantSize.size;

                switch (id)
                {
                    case NoSize: return (PacketLength) 0;
                    case ByteSize: return PacketLength.NextByte;
                    case ShortSize: return PacketLength.NextShort;
                    case Undefined: return PacketLength.Undefined;
                }

                throw new System.NotImplementedException();
            }
        }


        private OutBlob Data(int offset, int size)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IPacketDatabase>(_ => new MockPacketDb());

            var s = Mock.Server(services);
            var blob = new OutBlob(s.Services, size);

            blob.WriteCaret += offset;
            blob.ReadCaret += offset;

            return blob;
        }

        private void TestData(int start, int size, byte[] expected, Blob blob)
        {
            var idxInExpected = 0;
            for (var idxInBlob = start; idxInBlob < size; ) 
            {
                Assert.AreEqual(expected[idxInExpected], blob.Buffer[idxInBlob]);

                ++idxInExpected;
                ++idxInBlob;
            }
        }

        private void TestConstSize(int offset)
        {
            var d = Data(offset, 16);

            // write
            d.BeginPacket(MockPacketDb.ConstantSize.opcode);

            for (byte i = 0; i < MockPacketDb.ConstantSize.size; i++)
                d.Write(i);

            d.EndPacket();

            // create expected
            var headSize = 3;
            var expected = new byte[headSize];
            expected[0] = MockPacketDb.ConstantSize.opcode;
            expected[1] = 0;
            expected[2] = 1;

            TestData(offset, headSize, expected, d);
        }

        [TestMethod]
        public void NoSizeHeaderIfPacketSizeIsConstant()
        {
            TestConstSize(0);
        }

        [TestMethod]
        public void WithOffsetNoSizeHeaderIfPacketSizeIsConstant()
        {
            TestConstSize(5);
        }

        private void TestSizeNotMatchExceptionThrow(int offset, int bufferSize, byte opcode, int size)
        {
            var d = Data(offset, bufferSize);

            // write
            d.BeginPacket(opcode);

            for (byte i = 0; i < size; i++)
                d.Write(i);

            var ex = Assert.ThrowsException<PacketSizeDoesNotMatchWrittenSizeException>(() => d.EndPacket());
            Assert.AreEqual(ex.Opcode, opcode);
            Assert.AreEqual(ex.WrittenSize, size);
        }

        [TestMethod]
        public void ThrowIfOutOfRangeWhenWritingConstSizePacket()
        {
            TestSizeNotMatchExceptionThrow(0, 16, MockPacketDb.ConstantSize.opcode, MockPacketDb.ConstantSize.size * 2);
        }

        [TestMethod]
        public void ThrowIfOutOfRangeWhenWritingZeroSizePacket()
        {
            TestSizeNotMatchExceptionThrow(0, 16, MockPacketDb.NoSize, 1);
        }

        [TestMethod]
        public void OnlyWriteOpcodeOnNoSizePacket()
        {
            var d = Data(0, 2);

            d.BeginPacket(MockPacketDb.NoSize);
            d.EndPacket();

            Assert.AreEqual(d.WriteCaret, 1);
            Assert.AreEqual(d.Buffer[0], MockPacketDb.NoSize);
        }

        [TestMethod]
        public void ThrowIfWriteAnythingInNoSizePacket()
        {
            var d = Data(0, 4);

            d.BeginPacket(MockPacketDb.NoSize);
            d.Write(1);

            var ex = Assert.ThrowsException<PacketSizeDoesNotMatchWrittenSizeException>(() => d.EndPacket());
            Assert.AreEqual(ex.Opcode, MockPacketDb.NoSize);
            Assert.AreEqual(ex.WrittenSize, 1);
        }

        [TestMethod]
        public void WritePayloadSizeAsByteIfPacketIsTypeOfByteSize()
        {
            var d = Data(0, 16);

            d.BeginPacket(MockPacketDb.ByteSize);
            d.Write(4);
            d.Write(8);
            d.EndPacket();

            var expected = new byte[4];
            expected[0] = MockPacketDb.ByteSize;
            expected[1] = 2;
            expected[2] = 4;
            expected[3] = 8;

            TestData(0, expected.Length, expected, d);
        }

        [TestMethod]
        public void WritePayloadSizeAsShortIfPacketIsTypeOfShortSize()
        {
            var d = Data(0, 16);

            d.BeginPacket(MockPacketDb.ShortSize);
            d.Write(4);
            d.Write(8);
            d.EndPacket();

            var expected = new byte[5];
            expected[0] = MockPacketDb.ShortSize;
            expected[1] = 0;
            expected[2] = 2;
            expected[3] = 4;
            expected[4] = 8;

            TestData(0, expected.Length, expected, d);
        }

        [TestMethod]
        public void ThrowsWhenBeginningToWriteUndefinedPacket()
        {
            var d = Data(0, 2);
            Assert.ThrowsException<AttemptedToWriteUndefinedPacketException>(
                () => d.BeginPacket(MockPacketDb.Undefined));
        }
    }
}
