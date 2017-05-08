using CScape.Data;
using CScape.Game.Interface;
using CScape.Game.Item;

namespace CScape.Network.Packet
{
    public class MassSendInterfaceItems : IPacket
    {
        private readonly IInterfacedItemManager _itemManager;

        public const int Id = 53;

        public MassSendInterfaceItems(IInterfacedItemManager itemManager)
        {
            _itemManager = itemManager;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(53);

            stream.Write16((short)_itemManager.ContainerInterfaceId); // inteface id

            var sizePh = new PlaceholderHandle(stream, sizeof(short));

            // payload
            var nonEmptyUpperBound = 0; // the idx of the item that was not empty.
            for (var i = 0; i < _itemManager.Provider.Items.Length; i++)
            {
                var cur = _itemManager.Provider.Items[i];
                if (ItemHelper.IsEmpty(cur))
                {
                    // write 0 size, 0 id.
                    stream.Write16(0);
                    continue;
                }

                // write amount. If amount is > 255, write it as an int32
                if (cur.amount > 255)
                    stream.Write32(cur.amount);
                else
                    stream.Write((byte)cur.amount);

                // write id
                stream.Write16((short)cur.id);

                nonEmptyUpperBound = i;
            }

            sizePh.DoWrite(b => b.Write16((short)(nonEmptyUpperBound + 1))); // amount of items in the payload

            stream.EndPacket();
        }
    }
}