using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class MaxHealthChangedMessage : IGameMessage
    {
        public int PreviousHp { get; }
        public int NewHp { get; }

        public int EventId => (int) MessageId.MaxHealthChanged;

        public MaxHealthChangedMessage(int previousHp, int newHp)
        {
            PreviousHp = previousHp;
            NewHp = newHp;
        }
    }
}
