using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    public sealed class SystemChatMessagePacket : IPacket
    {
        private readonly string _msg;
        public const int Id = 253;

        public SystemChatMessagePacket(string msg)
        {
            _msg = msg;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);
            stream.WriteString(_msg);
            stream.EndPacket();
        }
    }
}