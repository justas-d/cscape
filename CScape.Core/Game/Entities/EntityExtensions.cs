using CScape.Core.Game.Entities.Message;

namespace CScape.Core.Game.Entities
{
    public static class EntityExtensions
    {
        public static void ShowParticleEffect(this Entity ent, ParticleEffect effect)
        {
            ent.SendMessage(
                new GameMessage(
                    null, GameMessage.Type.ParticleEffect, effect));
        }
    }
}
