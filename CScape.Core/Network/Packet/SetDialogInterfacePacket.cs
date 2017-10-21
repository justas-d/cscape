using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    public class SetDialogInterfacePacket : IPacket
    {
        private readonly short _id;

        public const int Id = 218;

        public static SetDialogInterfacePacket Close { get; } = new SetDialogInterfacePacket(-1);

        public SetDialogInterfacePacket(short id)
        {
            _id = id;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);
            stream.Write16(_id);
            stream.EndPacket();
        }
    }
}