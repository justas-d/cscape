using System.Collections;
using System.Threading.Tasks;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Dev.Tests.Impl
{
    public class MockLoop : IMainLoop
    {
        public class MockUpdateBatch : IUpdateBatch
        {
            public IUpdateQueue<IMovingEntity> Movement { get; } = new MockUpdateQueue<IMovingEntity>();
            public IUpdateQueue<Player> Player { get; } = new MockUpdateQueue<Player>();
            public IUpdateQueue<Npc> Npc { get; } = new MockUpdateQueue<Npc>();
            public IUpdateQueue<GroundItem> Item { get; } = new MockUpdateQueue<GroundItem>();

        }

        public class MockUpdateQueue<T> : IUpdateQueue<T> where T : IWorldEntity
        {
            public int Count => 0;
            public T Dequeue() => default(T);
            public void Enqueue([NotNull] T ent) { }
            public System.Collections.Generic.IEnumerator<T> GetEnumerator() => null;
            IEnumerator IEnumerable.GetEnumerator() => null;
        }

        public IUpdateBatch UpdHighFrequency { get; } = new MockUpdateBatch();
        public IUpdateBatch UpdLowFrequency { get; } = new MockUpdateBatch();

        public long ElapsedMilliseconds { get; }
        public long DeltaTime { get; }
        public long TickProcessTime { get; }
        public int TickRate { get; set; }
        public bool IsRunning { get; }

        public Task Run() => null;
    }
}