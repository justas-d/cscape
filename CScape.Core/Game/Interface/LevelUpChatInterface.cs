using CScape.Core.Game.Entities.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Message;

namespace CScape.Core.Game.Interface
{
    public sealed class LevelUpChatInterface : IGameInterface
    {
        private readonly string _skillName;
        private readonly int _newLevel;
        public int Id { get; }

        public void ShowForEntity(IEntity entity)
        {
            entity.SendMessage(InterfaceMessage.Close(this,
                new SetDialogInterfacePacket((short) Id),
                new SetInterfaceTextPacket(Id + 1, $"Congratulations, you just advanced a {_skillName} level."),
                new SetInterfaceTextPacket(Id + 2, $"Your {_skillName} level is now {_newLevel}")));
        }

        public void CloseForEntity(IEntity entity)
        {
            entity.SendMessage(InterfaceMessage.Close(this, SetDialogInterfacePacket.Close));
        }

        public void UpdateForEntity(IEntity entity)
        {

        }

        public void ReceiveMessage(IEntity entity, IGameMessage msg)
        {
            throw new System.NotImplementedException();
        }

        public LevelUpChatInterface(int id,
            string skillName, int newLevel)
        {
            _skillName = skillName;
            _newLevel = newLevel;
            Id = id;
        }


        public bool Equals(IGameInterface other)
            => Id == other.Id;
    }
}
