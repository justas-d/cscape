using CScape.Core.Game.Entities.Component;
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

        /// <summary>
        /// Resolves vision between two entities, where the main entity (<see cref="main"/>) is the entity trying to see the other entity <see cref="oth"/>.'
        /// 1) Whatever any other circumstace may be, if <see cref="main"/> doesn't have a vision component OR does have one but cannot see <see cref="oth"/>, we regard that as both entities not seeing each other.
        /// 2) If <see cref="main"/> does have a vision component and can see <see cref="oth"/>, but oth does not have a vision component, then we leave it as that and return true.
        /// 3) If <see cref="main"/> does see <see cref="oth"/> but oth has a vision component and cannot see <see cref="main"/>, we regard that as both entities not being able to see each other.
        /// </summary>
        /// <param name="main">The main entity around which the sight evaluation will be centered upon.</param>
        /// <param name="oth">The target entity.</param>
        /// <returns>True if both entities can see each other, false otherwise.</returns>
        public static bool CanSee(this Entity main, Entity oth)
        {
            var mainVision = main.Components.Get<VisionComponent>();
            var othVision = oth.Components.Get<VisionComponent>();

            // 1
            if (mainVision == null) return false;

            // 3
            if (othVision != null)
                return mainVision.CanSee(oth) && othVision.CanSee(main);

            // 1 & 2
            return mainVision.CanSee(oth);
        }
    }
}
