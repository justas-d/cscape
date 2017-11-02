using CScape.Models.Game.Entity;

namespace CScape.Core.Game.Entities
{
    public interface INpcComponent : IEntityComponent
    {
        /// <summary>
        /// The implementation specific definition id of the npc.
        /// </summary>
        short DefinitionId { get; }

        /// <summary>
        /// The instance id of the npc.
        /// </summary>
        short InstanceId { get; }

        /// <summary>
        /// Changes the definition id of the npc.
        /// </summary>
        void ChangeDefinitionId(short newId);
    }
}