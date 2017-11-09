namespace CScape.Core.Network.Packet
{
    public sealed class ShowSidebarInterfacePacket : IPacket
    {
        private readonly short _id;
        private readonly byte _sidebarIndex;
        public const int Id = 71;

        public ShowSidebarInterfacePacket(short id, byte sidebarIndex)
        {
            _id = id;
            _sidebarIndex = sidebarIndex;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16(_id);
            stream.Write(_sidebarIndex);

            stream.EndPacket();
        }
    }
}