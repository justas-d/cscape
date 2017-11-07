using CScape.Models.Game.Entity;

namespace CScape.Dev.Tests.ModelTests.Mock
{
    public sealed class TestMessage : IGameMessage
    {
        public int EventId => 0;
    }
}