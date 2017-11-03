using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Interface;

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
                new SetInterfaceTextPacket(Id + 1, $"Congratulations, you just advanced a {_skillName} level."),
                new SetInterfaceTextPacket(Id + 2, $"Your {_skillName} level is now {_newLevel}"),
                new SetDialogInterfacePacket((short)Id)));
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
            // TODO : LevelUpChatInterface isn't receiving button click events (the client isn't sending any??????)
            if (msg.EventId != (int) MessageId.ButtonClicked) return;
            var data = msg.AsButtonClicked();
            if (data.InterfaceId != Id) return;

            entity.SystemMessage($"Lvl up interface: {data.ButtonId}");
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
