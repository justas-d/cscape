using CScape.Models.Game.Entity;

namespace CScape.Models.Tests.Mock
{
    public sealed class TestMessage : IGameMessage
    {
        public int EventId => 0;
    }
}