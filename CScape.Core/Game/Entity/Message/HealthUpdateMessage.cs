using CScape.Models.Game.Entity;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class HealthUpdateMessage : IGameMessage
    {
        public int PreviousHp { get; }
        public int NewHp { get; }
    
        public int EventId => (int)MessageId.HealthChanged;

        public HealthUpdateMessage(int previousHp, int newHp)
        {
            PreviousHp = previousHp;
            NewHp = newHp;
        }
    }
}