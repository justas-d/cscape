using System.Collections.Generic;
using CScape.Core.Game.World;

namespace CScape.Core.Game.Entities.Directions
{
    /// <summary>
    /// Defines a directions provider which buffers directions added to it by the .Add call.
    /// It is not designed or intended to be used for multiple entities at the same time.
    /// For intended results, create one for every entity.
    /// </summary>
    public sealed class BufferedDirectionProvider : IDirectionsProvider
    {
        private readonly Queue<World.Direction> _buffer = new Queue<World.Direction>();
        private bool _isDone;

        public void SetDone() => _isDone = true;

        public void Add(World.Direction dir) => _buffer.Enqueue(dir);

        private DirectionDelta GetDirection()
        {
            return new DirectionDelta(_buffer.Count > 0 ? _buffer.Dequeue() : World.Direction.None);
        }

        public GeneratedDirections GetNextDirections(Entity ent) 
            => new GeneratedDirections(GetDirection(), GetDirection());
        
        public bool IsDone(Entity entity) => _isDone;
    }
}