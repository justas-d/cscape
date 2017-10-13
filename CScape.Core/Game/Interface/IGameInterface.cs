using System;
using System.Collections.Generic;
using CScape.Core.Network;

namespace CScape.Core.Game.Entities.Interface
{
    public interface IGameInterface : IEquatable<IGameInterface>
    {
        int Id { get; }

        IEnumerable<IPacket> GetShowPackets();
        IEnumerable<IPacket> GetClosePackets();
        IEnumerable<IPacket> GetUpdatePackets();

        void ReceiveMessage(GameMessage msg);
    }
}