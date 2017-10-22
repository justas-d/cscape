using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class ButtonClickMessage : IGameMessage
    {
        public int ButtonId { get; }
        public int InterfaceId { get; }

        public ButtonClickMessage(int buttonId, int interfaceId)
        {
            ButtonId = buttonId;
            InterfaceId = interfaceId;
        }

        public int EventId => (int)MessageId.ButtonClicked;
    }
}