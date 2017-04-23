using System.Collections.Generic;

namespace CScape.Game.Entity
{
    public sealed class IdPool
    {
        private uint _next = 0;

        // as the server's uptime grows, the _recycle queue in IdPool also grows
        // todo : new IdPool algorithm that is not memory intensive.
        // reset _next to n if _recycle contains all integers up to n? how would we know that?
        private readonly Queue<uint> _recycle = new Queue<uint>();

        public uint NextId()
        {
            if (_recycle.Count > 0)
                return _recycle.Dequeue();

            return _next++;
        }

        public void FreeId(uint id)
        {
            _recycle.Enqueue(id);
        }
    }
}