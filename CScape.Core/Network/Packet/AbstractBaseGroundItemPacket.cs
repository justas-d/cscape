using CScape.Core.Game.Items;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// The base packet for 
    /// </summary>
    public abstract class AbstractBaseGroundItemPacket : BaseGroundObjectPacket
    {
        protected ItemStack Item { get; }

        protected AbstractBaseGroundItemPacket(
            ItemStack item, int offX, int offY)
            : base(offX, offY)
        {
            if (item.IsEmpty())
                IsInvalid = true;
            
            if (!IsInvalid)
            {
                Item = item;
            }
        }
    }
}