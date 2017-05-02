namespace CScape
{
    public static class Constant
    {
        public static readonly int[] MaskForBit =
        {
            0, 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047, 4095, 8191, 16383,
            32767, 65535, 131071, 262143, 524287, 1048575, 2097151, 4194303, 8388607, 16777215, 33554431, 67108863,
            134217727, 268435455, 536870911, 1073741823, 2147483647,
        };

        public const byte StringNullTerminator = 10;

        public static class SyncMachineOrder
        {
            public const int Region = 0;
            public const int Observer = 1;
            public const int PlayerUpdate = 2;
            public const int NpcUpdate = 3;
            public const int Message = 4;
            public const int DebugStat = 5;
        }
    }
}
