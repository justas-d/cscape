using CScape.Core.Data;
using CScape.Core.Game.Interface;
using CScape.Core.Game.Item;

namespace CScape.Core.Network.Packet
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
            stream.BeginPacket(Id);

            stream.Write16((short) _itemManager.Id);

            // find upper bound in inventory
            var upperBoundIdx = 0;
            for (var i = 0; i < Provider.Count; i++)
            {
                if(Provider.IsEmptyAtIndex(i)) continue;
                upperBoundIdx = i;
            }

            // write payload
            upperBoundIdx += 1;
            stream.Write16((short)upperBoundIdx);

            for (var i = 0; i < upperBoundIdx; i++)
            {
                if (Provider.IsEmptyAtIndex(i))
                {
                    stream.Write(0); // amnt
                    stream.Write16(0); // idz
                    continue;
                }

                // write amount. If amount is > 255, write it as an int32
                stream.WriteByteInt32Smart(Provider.GetAmount(i));

                // write id
                stream.Write16((short)Provider.GetId(i));
            }

            stream.EndPacket();
        }
    }
}