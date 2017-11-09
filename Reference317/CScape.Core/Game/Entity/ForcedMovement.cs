using CScape.Models.Game.World;

namespace CScape.Core.Game.Entity
{
    public class ForcedMovement
    {
        public (byte x, byte y) Start { get; }
        public (byte x, byte y) End { get; }
        public (byte x, byte y) Duration { get; }
        public Direction Direction { get; }

        /// <param name="direction">Domain: [0; 4]</param>
        private ForcedMovement(
            Direction direction, (byte x, byte y) start,
            (byte x, byte y) end, (byte x, byte y) duration)
        {
            Direction = direction;
            Start = start;
            End = end;
            Duration = duration;
        }
    }
}