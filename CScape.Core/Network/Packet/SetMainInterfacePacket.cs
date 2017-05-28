using CScape.Core.Data;

namespace CScape.Core.Network
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