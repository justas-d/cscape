using CScape.Core.Game.Entity;
using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class NewPlayerAppearanceMessage : IGameMessage
    {
        public PlayerAppearance Appearance { get; }
        public int EventId => (int) MessageId.AppearanceChanged;

        public NewPlayerAppearanceMessage(PlayerAppearance appearance)
        {
            Appearance = appearance;
        }
    }
}
