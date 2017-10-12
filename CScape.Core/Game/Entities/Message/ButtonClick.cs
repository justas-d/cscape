namespace CScape.Core.Game.Entities.Message
{
    public sealed class ButtonClick
    {
        public int ButtonId { get; }
        public int InterfaceId { get; }

        public ButtonClick(int buttonId, int interfaceId)
        {
            ButtonId = buttonId;
            InterfaceId = interfaceId;
        }
    }
}