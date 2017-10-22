using CScape.Models.Game.Message;

namespace CScape.Core.Game.Entities
{
    public sealed class DefinitionChangeMessage : IGameMessage
    {
        public int EventId => (int)MessageId.DefinitionChange;
        public short Definition { get; }

        public DefinitionChangeMessage(short def)
        {
            Definition = def;
        }
    }
}