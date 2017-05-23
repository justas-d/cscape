using CScape.Core.Data;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// Encodes a packet which spawns a ground item that has a pid associated with it.
    /// Any players who have the same id as the one given to this packet
    /// will not see the item.
    /// </summary>
    public class SpawnGroundItemWithPidPacket : AbstractBaseGroundItemPacket
    {
        private readonly short _pid;

        public const int Id = 215;

        public SpawnGroundItemWithPidPacket(
            (int id, int amount) item,
            byte offX, byte offY, short pid)
            : base(item, (int) offX, (int) offY)
        {
            _pid = pid;
        }

        public override void Send(OutBlob stream)
        {
            if (IsInvalid) return;

            stream.BeginPacket(Id);

            stream.Write16(ItemId);
            stream.Write(PackedPos);
            stream.Write16(_pid);
            stream.Write16(Amount);

            stream.EndPacket();
        }
    }
}