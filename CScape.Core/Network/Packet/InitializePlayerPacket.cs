using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    public sealed class InitializePlayerPacket : IPacket
    {
        private readonly int _pid;
        private readonly bool _isMember;


        public const int Id = 249;

        public InitializePlayerPacket(int pid, bool isMember)
        {
            _pid = pid;
            _isMember = isMember;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(249);

            stream.Write((byte)(_isMember ? 1 : 0));
            stream.Write16((short)_pid);

            stream.EndPacket();
        }
    }
}