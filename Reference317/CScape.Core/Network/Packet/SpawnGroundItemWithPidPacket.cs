using CScape.Core.Extensions;
using CScape.Models.Data;
using CScape.Models.Game.Item;

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
            ItemStack item,
            (int x, int y) off,
            short pid)
            : base(item, off.x, off.y)
        {
            _pid = pid;
        }

        protected override void InternalSend(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16((short)Item.Id.ItemId);
            stream.Write(PackedPos);
            stream.Write16(_pid);
            stream.Write16((short)Item.Amount.Clamp(0, short.MaxValue));

            stream.EndPacket();
        }
    }
}