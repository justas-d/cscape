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
            : base(offX, offY)
        {
            if (ItemHelper.IsEmpty(item))
                IsInvalid = true;

            if (!IsInvalid)
            {
                ItemId = (short) item.id;
                Amount = (short) Utils.Clamp(item.amount, 0, ushort.MaxValue);
            }
        }
    }
}