using System;

namespace CScape.Basic.Cache
{
    public sealed class CacheDataBlockReadFailureException : Exception
    {
        public override string Message { get; }
        public int ReceivedValue { get; }
        public int ExpectedValue { get; }
        public int BlockNumber { get; }

        public CacheDataBlockReadFailureException(
            string message, int receivedValue, int expectedValue, int blockNumber)
        {
            Message = message;
            ReceivedValue = receivedValue;
            ExpectedValue = expectedValue;
            BlockNumber = blockNumber;
        }
    }
}