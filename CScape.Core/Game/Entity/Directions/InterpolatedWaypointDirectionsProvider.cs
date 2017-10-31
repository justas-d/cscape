using System;
using System.Collections.Generic;
using System.Linq;
using CScape.Models.Extensions;
using CScape.Models.Game;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Directions;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Directions
{
    /// <summary>
    /// Designed for continued use by one entity.
    /// </summary>
    public sealed class InterpolatedWaypointDirectionsProvider : IDirectionsProvider
    {
        [NotNull]
        public IList<IPosition> Waypoints { get; }

        private int _currentWaypointIndex = 0;

        public InterpolatedWaypointDirectionsProvider([NotNull] IList<IPosition> waypoints)
        {
            if (waypoints.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(waypoints));
            Waypoints = waypoints ?? throw new ArgumentNullException(nameof(waypoints));
        }

        private GeneratedDirections GenerateDirections(IEntity ent)
        {
            // generate directions
            var data = PathingUtils.WalkTo(
                ent.GetTransform(), Waypoints[_currentWaypointIndex])
                    .Take(2)
                    .ToArray();

            return new GeneratedDirections(data[0], data[1]);
        }

        public GeneratedDirections GetNextDirections(IEntity ent)
        {
            // recursion made iteration
            while (true)
            {
                // first, check if we have any waypoints left.
                if (_currentWaypointIndex >= Waypoints.Count)
                    return GeneratedDirections.Noop;

                // we're good, generate dirs to waypoint
                var dirs = GenerateDirections(ent);

                // if the data is a noop, we're standing on our current waypoint
                if (dirs.IsNoop())
                {
                    // we'll need to advance to the next waypoint.
                    _currentWaypointIndex++;

                    // this waypoint is done, recurse in in order to walk toward the next waypoint.
                    continue;
                }

                // if the run dir is a noop, we'll try to interpolate again 
                if (dirs.Run.IsNoop())
                {
                    _currentWaypointIndex++;
                    var nextDirs = GetNextDirections(ent);

                    // concatenate nextDirs walk with our run.
                    return new GeneratedDirections(dirs.Walk, nextDirs.Walk);
                }

                return dirs;
            }
        }

        public bool IsDone(IEntity entity)
        {
            // last waypoint is the target.
            return entity.GetTransform().Equals(Waypoints.Last());
        }
    }
}