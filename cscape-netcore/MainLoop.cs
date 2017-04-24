using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CScape.Game.Entity;
using CScape.Network.Packet;
using JetBrains.Annotations;

namespace CScape
{
    public sealed class MainLoop
    {
        /// <summary>
        /// Defines a queue for entities that need to be updated.
        /// Only one of the same entity can exist in the queue.
        /// </summary>
        public sealed class UniqueEntUpdateQueue<T> : IEnumerable<T> where T : AbstractEntity
        {
            private readonly HashSet<uint> _idSet = new HashSet<uint>();
            private readonly Queue<T> _entQueue = new Queue<T>();

            public void Enqueue(T ent)
            {
                if (!_idSet.Add(ent.UniqueEntityId))
                    return;

                _entQueue.Enqueue(ent);
            }

            public T Dequeue()
            {
                var obj = _entQueue.Dequeue();
                _idSet.Remove(obj.UniqueEntityId);
                return obj;
            }

            public int EntCount => _entQueue.Count;

            public IEnumerator<T> GetEnumerator()
            {
                return _entQueue.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        // update queues
        [NotNull] public UniqueEntUpdateQueue<AbstractEntity> Movement { get; } = new UniqueEntUpdateQueue<AbstractEntity>();
        [NotNull] public UniqueEntUpdateQueue<Player> Player { get; } = new UniqueEntUpdateQueue<Player>();
        [NotNull] public ConcurrentQueue<IPlayerLogin> LoginQueue { get; } = new ConcurrentQueue<IPlayerLogin>();

        [NotNull] public GameServer Server { get; }
        [NotNull] public Logger Log => Server.Log;
        [NotNull] private readonly Stopwatch _tickWatch = new Stopwatch();

        public PacketDispatch PacketDispatch { get; }

        public long ElapsedMilliseconds => _tickWatch.ElapsedMilliseconds;

        public long DeltaTime { get; private set; }
        public int MaxTickTime { get; }

        public MainLoop([NotNull] GameServer server, int tickTime)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            PacketDispatch = new PacketDispatch(this);
            MaxTickTime = tickTime;
        }

        public async Task Run()
        {
            //TODO: bool to terminate main loop
            // todo : exception handle all over the main loop

            Log.Normal(this, "Starting main loop...");

            while (true)
            {
                _tickWatch.Start();

                // handle new logins
                while (LoginQueue.TryDequeue(out IPlayerLogin login))
                    login.Transfer(this);

                // get & parse their data
                foreach (var p in Player)
                {
                    // get 
                    p.Connection.FlushInput();

                    // parse
                    foreach (var pack in PacketParser.Parse(p, Server, p.Connection.InCircularStream))
                        PacketDispatch.Handle(p, pack.Opcode, pack.Packet);
                }

                // movement updates
                var size = Movement.EntCount;
                for (var i = 0; i < size; ++i)
                    Movement.Dequeue().Movement?.Update();

                // write & send
                // todo : offload write & send to a different thread?
                foreach (var p in Player)
                {
                    // write our data
                    foreach (var sync in p.Connection.SyncMachines)
                        sync.Synchronize(p.Connection.OutStream);

                    // send our data
                    p.Connection.SendOutStream();
                }

                // player entity updating.
                size = Player.EntCount;
                for (var i = 0; i < size; ++i)
                    Player.Dequeue().Update(this);

                // handle tick delays
                _tickWatch.Stop();
                _tickWatch.Reset();

                var waitTime = Math.Abs(MaxTickTime - Convert.ToInt32(_tickWatch.ElapsedMilliseconds));
                var overtime = waitTime < MaxTickTime;
                if (overtime)
                    Log.Warning(this, $"Tick process time too slow! need to wait for {waitTime}ms. Tick target ms: {MaxTickTime}ms.");
                else
                    await Task.Delay(waitTime);

                DeltaTime = waitTime;
            }
        }
    }
}