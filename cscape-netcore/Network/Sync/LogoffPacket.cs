namespace CScape.Network.Sync
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

    public sealed class LogoffPacket : IPacket
    {
        public const int Id = 109;

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);
            stream.EndPacket();
        }

        public static LogoffPacket Static => new LogoffPacket();
    }
}