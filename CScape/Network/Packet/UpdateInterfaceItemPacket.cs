using System.Collections.Generic;
using System.Linq;
using CScape.Data;
using CScape.Game.Interface;

namespace CScape.Network.Packet
{
    public sealed class UpdateInterfaceItemPacket : IPacket
    {
        private readonly IContainerInterface _itemManager;
        private readonly ICollection<int> _indicies;

        public const int Id = 34;

        public UpdateInterfaceItemPacket(IContainerInterface itemManager, 
            ICollection<int> indicies)
        {
            _itemManager = itemManager;
            _indicies = indicies;
        }

        public void Send(OutBlob stream)
        {
            // don't write the packet if no indicies have been passed.
            if (!_indicies.Any())
                return;

            stream.BeginPacket(Id);

            stream.Write16((short)_itemManager.Id);

            foreach (var i in _indicies)
            {
                // write index
                if (i < 128)
                    stream.Write((byte)i);
                else
                    stream.Write16((short)i);

                // write id
                stream.Write16((short)_itemManager.Items.Provider.Ids[i]);

                // write size as byte-int32 smart
                stream.WriteByteInt32Smart(_itemManager.Items.Provider.Amounts[i]);
            }

            stream.EndPacket();
        }
    }
}