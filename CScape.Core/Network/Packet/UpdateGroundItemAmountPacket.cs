using CScape.Core.Data;
using CScape.Core.Game.Entity;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// Encodes a packet which updates the amount of items are in an item stack identified by it's id and amount before change at the given packed region x/y.
    /// </summary>
    public class UpdateGroundItemAmountPacket : AbstractBaseGroundItemPacket
    {
        private readonly short _newAmount;
        public const int Id = 84;

        public UpdateGroundItemAmountPacket(
            GroundItem item,
            (int x, int y) off)
            :this(item.ItemId, item.ItemAmount, item.OldAmount , off.x, off.y)
        {
            
        }

        public UpdateGroundItemAmountPacket(
            int id, int amount, 
            int newAmount,  
            int offX, int offY) 
            : base(id, amount, offX, offY)
        {
            _newAmount = (short)Utils.Clamp(newAmount, 0, ushort.MaxValue);
        }

        protected override void InternalSend(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write(PackedPos);
            stream.Write16(ItemId);
            stream.Write16(Amount);
            stream.Write16(_newAmount);

            stream.EndPacket();
        }
    }
}