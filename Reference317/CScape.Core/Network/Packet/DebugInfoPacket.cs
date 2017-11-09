namespace CScape.Core.Network.Packet
{
    public sealed class DebugInfoPacket : IPacket
    {
        private readonly int _dt;
        private readonly int _processTime;
        public const byte Id = 2;

        public DebugInfoPacket(int dt, int processTime)
        {
            _dt = dt;
            _processTime = processTime;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16((short)_dt);
            stream.Write16((short)_processTime);

            stream.EndPacket();
        }
    }
}