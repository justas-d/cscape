using CScape.Core.Data;

namespace CScape.Core.Network.Packet
{
    public sealed class SetPlayerOptionPacket : IPacket
    {
        public static SetPlayerOptionPacket Follow => new SetPlayerOptionPacket(1, false, "Follow");
        public static SetPlayerOptionPacket TradeWith => new SetPlayerOptionPacket(2, false, "Trade with");
        public static SetPlayerOptionPacket Report => new SetPlayerOptionPacket(3, false, "Report");
        public static SetPlayerOptionPacket Attack  => new SetPlayerOptionPacket(4, true, "Attack");

        public int IndexMin = 1;
        public int IndexMax = 5;

        private readonly byte _index;
        private readonly byte _isOnTop;
        private readonly string _msg;

        public const int Id = 104;

        public SetPlayerOptionPacket(byte index, bool isOnTop, string msg)
        {
            _index = index;
            _isOnTop = isOnTop ? (byte)1 : (byte)0;
            _msg = msg;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);    
            stream.Write(_index);
            stream.Write(_isOnTop);
            stream.WriteString(_msg);

            stream.EndPacket();
        }
    }
}