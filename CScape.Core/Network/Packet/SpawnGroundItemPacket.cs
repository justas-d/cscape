using CScape.Core.Data;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// Encodes an a packet which spawns a ground item.
    /// </summary>
    public class SpawnGroundItemPacket : AbstractBaseGroundItemPacket
    {
        public const int Id = 44;

        public SpawnGroundItemPacket(
            (int id, int amount) item,
            byte offX, byte offY)
            : base(item, (int) offX, (int) offY)
        {

        }

        public override void Send(OutBlob stream)
        {
            if (IsInvalid) return;

            stream.BeginPacket(Id);

            stream.Write16(ItemId);
            stream.Write16(Amount);
            stream.Write(PackedPos);

            stream.EndPacket();
        }
    }
}