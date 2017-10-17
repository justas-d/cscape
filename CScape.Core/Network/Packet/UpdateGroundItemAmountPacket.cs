using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Items;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// Encodes a packet which updates the amount of items are in an item stack identified by it's id and amount before change at the given packed region x/y.
    /// </summary>
    public class UpdateGroundItemAmountPacket : AbstractBaseGroundItemPacket
    {
        private readonly short _oldAmount;
        public const int Id = 84;

        public UpdateGroundItemAmountPacket(
            ItemStack item,
            int oldAmount,
            (int x, int y) off)
            : base(item, off.x, off.y)
        {
            _oldAmount = (short)oldAmount.Clamp(0, ushort.MaxValue);
        }

        protected override void InternalSend(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write(PackedPos);
            stream.Write16((short)Item.Id.ItemId);
            stream.Write16((short)Item.Amount.Clamp(0, short.MaxValue));
            stream.Write16(_oldAmount);

            stream.EndPacket();
        }
    }
}