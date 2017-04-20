namespace cscape
{
    /// <summary>
    /// If not in range, represents the length of bytes in the packet id. If in range, represents the type of length.
    /// </summary>
    public enum PacketLength
    {
        NextByte = -1,
        NextShort = -2,
        Undefined = -3
    }
}