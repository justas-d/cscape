using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using CScape.Core.Injection;

namespace CScape.Basic.Model
{
    public class NullCollisionProvider : ICollisionProvider
    {
        public bool CanWalk(IPosition position, Direction inDirection)
        {
            return true;
        }
    }
}
