using System.Collections.Generic;
using CScape.Models.Game;
using CScape.Models.Game.World;

namespace CScape.Models.Extensions
{
    public static class PathingUtils
    {
        /// <summary>
        /// Generates direction deltas to move an entity from the beginning position <see cref="us"/> to the target ending position <see cref="target"/>.
        /// This algorithm does not generate deltas which navigate around obsticles.
        /// This algorithim will cause undefined behaviour if any other <see cref="DirectionDelta"/> were to be applied to entity for which directions are being generated.
        /// If <see cref="us"/> and <see cref="target"/> are equal, this algorithm will return noop directions.
        /// </summary>
        /// <param name="us">The starting position.</param>
        /// <param name="target">The goal.</param>
        /// <returns>A lazy <see cref="IEnumerable{T}"/> containing DirectionDeltas that lead to <see cref="target"/></returns>
        public static IEnumerable<DirectionDelta> WalkTo(IPosition us, IPosition target)
        {
            /*
             * Starting position is only used once to initialize a starting location, 
             * to which the algorithm will add to during each evaluation of the IEnumerable
             * in order to keep track of where the entity is.
             */
            var x = us.X;
            var y = us.Y;
            var z = us.Z;
            
            while (x != target.X &&
                   y != target.Y)
            {
                if (z != target.Z)
                {
                    yield return DirectionDelta.Noop;
                }
                else
                {
                    /* if target is higher than us, we have to walk up
                     * and if the target is lower than us, then we have to walk down.
                     */
                    /* We do this comparison for both X and Y axis so we know in which
                     * direction we will have to walk for both of them
                     */
                    var diffX = x < target.X ? (sbyte)1 : (sbyte)-1;
                    var diffY = y < target.Y ? (sbyte)1 : (sbyte)-1;

                    // We return the delta that will get us closer to the target.

                    /* We do have to check if one of the axis coordinates is the same for us and the target,
                     * since if we do share a mutual coordinate, we do not have to move. 
                     * Therefore we return 0 if we have mututal coordinates and the diff of that axis if we do not (in order to get closer to it)
                     */
                     var delta = new DirectionDelta(
                        target.X == x ? (sbyte)0 : diffX,
                        target.Y == y ? (sbyte)0 : diffY);

                    // sync local x y's
                    x += delta.X;
                    y += delta.Y;

                    yield return delta;
                }
            }
        }
    }
}
