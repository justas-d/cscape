using CScape.Core.Game.Item;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// The base packet for 
    /// </summary>
    public abstract class AbstractBaseGroundItemPacket : BaseGroundObjectPacket
    {
        protected short ItemId { get; }
        protected short Amount { get; }

        protected AbstractBaseGroundItemPacket(
            (int id, int amount) item, int offX, int offY)
            : this(item.id, item.amount, offX, offY)
        {
        }

        protected AbstractBaseGroundItemPacket(
            int id, int amount, int offX, int offY)
            : base(offX, offY)
        {
            if (ItemHelper.IsEmpty(id, amount))
                IsInvalid = true;

            if (!IsInvalid)
            {
                ItemId = (short) id;
                Amount = (short) Utils.Clamp(amount, 0, ushort.MaxValue);
            }
        }
    }
}