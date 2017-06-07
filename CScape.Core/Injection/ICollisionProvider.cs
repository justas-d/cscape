using CScape.Core.Game.Entity;
using CScape.Core.Game.World;

namespace CScape.Core.Injection
{
    public interface ICollisionProvider
    {
        bool CanWalk(
            IPosition position,
            Direction inDirection);

        // todo : projectile collision
    }
}