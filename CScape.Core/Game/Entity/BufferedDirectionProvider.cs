using System.Collections.Generic;
using System.Linq;
using CScape.Core.Game.World;

namespace CScape.Core.Game.Entity
{
    public sealed class BufferedDirectionProvider : IDirectionsProvider
    {
        private readonly Queue<Direction> _buffer = new Queue<Direction>();
        private bool _isDone;

        public void Add(Direction dir) => _buffer.Enqueue(dir);

        public void Done() => _isDone = true;

        (sbyte x, sbyte y) IDirectionsProvider.GetNextDir()
        {
            if (Enumerable.Any<Direction>(_buffer))
                return DirectionHelper.GetDelta(_buffer.Dequeue());

            return DirectionHelper.NoopDelta;
        }

        bool IDirectionsProvider.IsDone() => _isDone;
        void IDirectionsProvider.Dispose() { }
    }
}