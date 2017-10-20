using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entities
{
    public sealed class DefinitionChangeMessage : IGameMessage
    {
        public int EventId => MessageId.DefinitionChange;
        public int Definition { get; }

        public DefinitionChangeMessage(int def)
        {
            Definition = def;
        }
    }
}