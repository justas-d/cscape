using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Extensions
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Sends a system message to the entity.
        /// </summary>
        public static void SystemMessage(this IEntity ent, [NotNull] string msg, SystemMessageFlags flags = SystemMessageFlags.None)
        {
            if (string.IsNullOrEmpty(msg)) return;

            ent.SendMessage(
                new GameMessage(
                    null, GameMessage.Type.NewSystemMessage, new SystemMessage(msg, flags)));
        }

    }
}
