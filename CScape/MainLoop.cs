using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CScape.Game.Entity;
using CScape.Network;
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
        public sealed class UniqueEntUpdateQueue<T> : IEnumerable<T> where T : IEntity
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
        [NotNull] public UniqueEntUpdateQueue<IMovingEntity> Movement { get; } = new UniqueEntUpdateQueue<IMovingEntity>();
        [NotNull] public UniqueEntUpdateQueue<Player> Player { get; } = new UniqueEntUpdateQueue<Player>();
        [NotNull] public ConcurrentQueue<IPlayerLogin> LoginQueue { get; } = new ConcurrentQueue<IPlayerLogin>();

        [NotNull] public GameServer Server { get; }
        [NotNull] public Logger Log => Server.Log;
        [NotNull] private readonly Stopwatch _tickWatch = new Stopwatch();

        public PacketDispatch PacketDispatch { get; }

        public long ElapsedMilliseconds => _tickWatch.ElapsedMilliseconds;

        public long DeltaTime { get; private set; }
        public long TickProcessTime { get; private set; }
        public int TickRate { get; set; }

        public MainLoop([NotNull] GameServer server, int tickRate)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            PacketDispatch = new PacketDispatch(this);
            TickRate = tickRate;
        }

        public async Task Run()
        {
            //TODO: bool to terminate main loop
            // todo : exception handle all over the main loop

            Log.Normal(this, "Starting main loop...");

            while (true)
            {
                _tickWatch.Restart();

                //================================================

                // handle new logins
                while (LoginQueue.TryDequeue(out IPlayerLogin login))
                    login.Transfer(this);

                //================================================

                // get & parse their data
                foreach (var p in Player)
                {
                    // get 
                    p.Connection.FlushInput();

                    // parse
                    foreach (var pack in PacketParser.Parse(p, Server, p.Connection.InCircularStream))
                        PacketDispatch.Handle(p, pack.Opcode, pack.Packet);
                }

                //================================================

                // movement updates
                var size = Movement.EntCount;
                for (var i = 0; i < size; ++i)
                    Movement.Dequeue().Movement.Update();

                //================================================

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

                //================================================

                // player entity updating.
                size = Player.EntCount;
                for (var i = 0; i < size; ++i)
                    Player.Dequeue().Update(this);

                //================================================

                // handle tick delays
                TickProcessTime = _tickWatch.ElapsedMilliseconds;
                var waitTime = Math.Abs(TickRate - Convert.ToInt32(TickProcessTime));
                await Task.Delay(waitTime);

                DeltaTime = waitTime + TickProcessTime;
            }
        }
    }
}