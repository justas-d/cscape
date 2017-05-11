using CScape.Data;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Network.Packet
{
    public sealed class InitializePlayerPacket : IPacket
    {
        [NotNull] private readonly Player _player;

        public const int Id = 249;

        public InitializePlayerPacket([NotNull]Player player)
        {
            _player = player;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(249);

            stream.Write((byte)(_player.IsMember ? 1 : 0));
            stream.Write16((short)_player.Pid);

            stream.EndPacket();
        }
    }
}