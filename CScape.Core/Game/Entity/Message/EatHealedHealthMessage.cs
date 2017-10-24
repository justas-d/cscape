using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class EatHealedHealthMessage : IGameMessage
    {
        public int NewHp { get; }
        public int HealedAmount { get; }

        public int EventId => (int)MessageId.EatHealedHealth;

        public EatHealedHealthMessage(int newHp, int healedAmount)
        {
            NewHp = newHp;
            HealedAmount = healedAmount;
        }
    }
}