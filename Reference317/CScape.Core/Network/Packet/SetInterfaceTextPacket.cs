using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    public class SetInterfaceTextPacket : IPacket
    {
        private readonly int _id;
        private readonly string _text;

        public const int Id = 126;

        public SetInterfaceTextPacket(int id, string text)
        {
            _id = id;
            _text = text;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.WriteString(_text);
            stream.Write16((short)_id);

            stream.EndPacket();
        }
    }
}