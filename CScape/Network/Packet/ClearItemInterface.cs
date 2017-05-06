using CScape.Data;

namespace CScape.Network.Packet
{
    public sealed class ClearItemInterface : IPacket
    {
        private readonly int _containerId;
        public const int Id = 72;

        public ClearItemInterface(int containerId)
        {
            _containerId = containerId;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);
            stream.Write16((short)_containerId);
            stream.EndPacket();
        }
    }
}