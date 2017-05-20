using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;

namespace CScape.Core.Network.Handler
{
    public sealed class SwapItemPacketHandler : IPacketHandler
    {
            public byte[] Handles { get; } = { 214 };

        public void Handle(Player player, int opcode, Blob packet)
        {
            // read
            var interfaceIdx = packet.ReadInt16();
            var magic = packet.ReadByte();

            player.DebugMsg($"Swap iidx: {interfaceIdx} w/ magic: {magic}", ref player.DebugItems);

            var fromIdx = packet.ReadInt16();
            var toIdx = packet.ReadInt16();

            // swapping item A with item A is a no-op, skip.
            if (fromIdx == toIdx) return;

            player.DebugMsg($"Swap {fromIdx} -> {toIdx}", ref player.DebugItems);

            // get and verify container interface
            var container = player.Interfaces.TryGetById(interfaceIdx) as IContainerInterface;
            if (container == null)
            {
                player.Log.Warning(this, $"Attempted to switch in unregistered interface: {interfaceIdx}");
                return;
            }

            var swappable = container.Items as ISwappableItemManager;
            if (swappable == null)
            {
                player.Log.Warning(this, $"Attempted to switch in unswappable interface: {interfaceIdx}");
                return;
            }

            // check if idx are in range
            bool IsNotInRange(int val)
            {
                var ret = 0 > val || val >= container.Items.Size;
                if (ret) player.Log.Warning(this, $"Out of range swap indicies: {fromIdx} -> {toIdx} (max is {container.Items.Size})");
                return ret;
            }

            if (IsNotInRange(fromIdx)) return;
            if (IsNotInRange(toIdx)) return;

            // execute managed swap
            swappable.Swap(fromIdx, toIdx);
        }
    }
}
