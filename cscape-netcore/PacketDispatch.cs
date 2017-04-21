namespace cscape
{
    public sealed class PacketDispatch
    {
        // in: player, packet
        // out: gets sent to appropriate packet handler
        // cool part: scans for packet handler implementations in it's static constructor
    }
}