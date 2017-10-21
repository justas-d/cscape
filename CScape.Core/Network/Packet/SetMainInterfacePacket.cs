using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    public class SetMainInterfacePacket : IPacket
    {
        private readonly int _id;

        public const int Id = 97;

        public SetMainInterfacePacket(int id)
        {
            _id = id;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);
            stream.Write16((short)_id);
            stream.EndPacket();
        }
    }
}