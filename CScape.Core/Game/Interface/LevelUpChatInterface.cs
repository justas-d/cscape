using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Network;
using CScape.Core.Network.Packet;

namespace CScape.Core.Game.Interfaces
{
    public sealed class LevelUpChatInterface : IGameInterface
    {
        private readonly string _skillName;
        private readonly int _newLevel;
        public int Id { get; }

        public LevelUpChatInterface(int id,
            string skillName, int newLevel)
        {
            _skillName = skillName;
            _newLevel = newLevel;
            Id = id;
        }

        public IEnumerable<IPacket> GetShowPackets()
        {
            yield return new SetDialogInterfacePacket((short)Id);
            yield return new SetInterfaceTextPacket(Id + 1, $"Congratulations, you just advanced a {_skillName} level.");
            yield return new SetInterfaceTextPacket(Id + 2, $"Your {_skillName} level is now {_newLevel}");
        }

        public IEnumerable<IPacket> GetClosePackets()
        {
            yield return SetDialogInterfacePacket.Close;
        }

        public IEnumerable<IPacket> GetUpdatePackets() => Enumerable.Empty<IPacket>();
        
        public void ReceiveMessage(GameMessage msg)
        {
            
        }

        public bool Equals(IGameInterface other)
            => Id == other.Id;
    }
}
