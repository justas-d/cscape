using CScape.Data;
using CScape.Game.Interface;
using CScape.Game.Item;

namespace CScape.Network.Packet
{
    public sealed class MassSendInterfaceItemsPacket : IPacket
    {
        private readonly IContainerInterface _itemManager;
        private IItemProvider Provider => _itemManager.Items.Provider;

        public const int Id = 53;

        public MassSendInterfaceItemsPacket(IContainerInterface itemManager)
        {
            _itemManager = itemManager;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(53);

            stream.Write16((short)_itemManager.Id);

            var sizePh = new PlaceholderHandle(stream, sizeof(short));

            // payload
            var nonEmptyUpperBound = 0; // the idx of the item that was not empty.
            for (var i = 0; i < _itemManager.Items.Provider.Size; i++)
            {
                if (ItemHelper.IsEmptyAtIndex(Provider, i))
                {
                    // write 0 size, 0 id.
                    stream.Write16(0);
                    continue;
                }

                // write amount. If amount is > 255, write it as an int32
                stream.WriteByteInt32Smart(Provider.Amounts[i]);

                // write id
                stream.Write16((short) Provider.Ids[i]);

                nonEmptyUpperBound = i;
            }

            sizePh.DoWrite(b => b.Write16((short)(nonEmptyUpperBound + 1))); // amount of items in the payload

            stream.EndPacket();
        }
    }
}