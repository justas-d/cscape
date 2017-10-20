using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class HealthUpdateMessage : IGameMessage
    {
        public int PreviousHp { get; }
        public int NewHp { get; }
        public bool DidEat { get; }
        public int EventId => MessageId.HealthUpdate;

        public HealthUpdateMessage(int previousHp, int newHp, bool didEat)
        {
            PreviousHp = previousHp;
            NewHp = newHp;
            DidEat = didEat;
        }
    }
}
