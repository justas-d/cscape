using CScape.Core.Data;
using CScape.Core.Game.Interface;
namespace CScape.Core.Network.Packet
{
    public sealed class MassSendInterfaceItemsPacket : IPacket
    {
        private readonly int _id;
        private readonly IItemContainer _container;

        public const int Id = 53;

        public MassSendInterfaceItemsPacket(int id, IItemContainer container)
        {
            _id = id;
            _container = container;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16((short) _id);

            // find upper bound in inventory
            var upperBoundIdx = 0;
            for (var i = 0; i < _container.Provider.Count; i++)
            {
                if(_container.Provider[i].IsEmpty())
                    continue;

                upperBoundIdx = i;
            }

            // write payload
            upperBoundIdx += 1;
            stream.Write16((short)upperBoundIdx);

            for (var i = 0; i < upperBoundIdx; i++)
            {
                var item = _container.Provider[i];
                if (item.IsEmpty())
                {
                    stream.Write(0); // amnt
                    stream.Write16(0); // id
                    continue;
                }

                // write amount. If amount is > 255, write it as an int32
                stream.WriteByteInt32Smart(item.Amount);

                // write id
                stream.Write16((short)item.Id.ItemId);
            }

            stream.EndPacket();
        }
    }
}