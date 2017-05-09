using System.Collections.Generic;
using System.Linq;
using CScape.Data;
using CScape.Game.Interface;

namespace CScape.Network.Packet
{
    public sealed class CloseSidebarInterface : IPacket
    {
        private readonly int _sidebarInterfaceIndex;
        public const int Id = 71;

        public CloseSidebarInterface(int sidebarInterfaceIndex)
        {
            _sidebarInterfaceIndex = sidebarInterfaceIndex;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write16(-1);
            stream.Write((byte)_sidebarInterfaceIndex);

            stream.EndPacket();
        }
    }

    public sealed class ShowSidebarInterfacePacket : IPacket
    {
        private readonly GameServer _server;
        private readonly IManagedInterface _interf;
        public const int Id = 71;

        public ShowSidebarInterfacePacket(GameServer server, IManagedInterface interf)
        {
            _server = server;
            _interf = interf;
        }

        public void Send(OutBlob stream)
        {
            // verify interf type
            if (_interf.Info.Type != InterfaceInfo.InterfaceType.Sidebar)
            {
                _server.Log.Warning(this, $"Tried showing interface that is not configured to be a sidebar interface. Id: {_interf.InterfaceId}");
                return;
            }

            stream.BeginPacket(Id);

            stream.Write16((short)_interf.InterfaceId);
            stream.Write((byte)_interf.Info.SidebarIndex);

            stream.EndPacket();
        }
    }

    public sealed class UpdateInterfaceItemPacket : IPacket
    {
        private readonly ISyncedItemManager _itemManager;
        private readonly ICollection<int> _indicies;

        public const int Id = 34;

        public UpdateInterfaceItemPacket(ISyncedItemManager itemManager, 
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

            stream.Write16((short)_itemManager.InterfaceId);

            foreach (var i in _indicies)
            {
                var item = _itemManager.Provider.Items[i];

                // write index
                if (i < 128)
                    stream.Write((byte)i);
                else
                    stream.Write16((short)i);

                // write id
                stream.Write16((short)item.id);

                // write size as byte-int32 smart
                stream.WriteByteInt32Smart(item.amount);
            }

            stream.EndPacket();
        }
    }
}