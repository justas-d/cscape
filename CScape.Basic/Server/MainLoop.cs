using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CScape.Core;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using CScape.Core.Network;
using JetBrains.Annotations;

namespace CScape.Basic.Server
{
    public sealed class MainLoop : IMainLoop, IDisposable
    {
        /// <summary>
        /// Defines a queue for entities that need to be updated.
        /// Only one of the same entity can exist in the queue.
        /// </summary>
        public sealed class UniqueEntUpdateQueue<T> : IUpdateQueue<T> where T : IEntity
        {
            private readonly HashSet<uint> _idSet = new HashSet<uint>();
            private readonly Queue<T> _entQueue = new Queue<T>();
            public int Count => _entQueue.Count;

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

            public IEnumerator<T> GetEnumerator() => _entQueue.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        // update queues
        public IUpdateQueue<IMovingEntity> Movement { get; } = new UniqueEntUpdateQueue<IMovingEntity>();
        public IUpdateQueue<Player> Player { get; } = new UniqueEntUpdateQueue<Player>();

        [NotNull] private readonly Stopwatch _tickWatch = new Stopwatch();
        private readonly IPacketDispatch _dispatch;
        private readonly IPacketParser _parser;
        private readonly ILoginService _login;
        private readonly ILogger _log ;

        public long ElapsedMilliseconds => _tickWatch.ElapsedMilliseconds;

        public long DeltaTime { get; private set; }
        public long TickProcessTime { get; private set; }
        public int TickRate { get; set; }

        public MainLoop(IServiceProvider services)
        {
            _log = services.ThrowOrGet<ILogger>();
            _login = services.ThrowOrGet<ILoginService>();
            _dispatch = services.ThrowOrGet<IPacketDispatch>();
            _parser = services.ThrowOrGet<IPacketParser>();
            TickRate = services.ThrowOrGet<IGameServerConfig>().TickTime;
        }

        public bool IsRunning { get; private set; } = true;

        public async Task Run()
        {
            _log.Normal(this, "Starting main loop...");

            // todo : exception handle all over the main loop
            try
            {
                while (IsRunning)
                {
                    _tickWatch.Restart();

                    //================================================

                    // handle new logins
                    IPlayerLogin next;
                    while ((next = _login.TryGetNext()) != null)
                        next.Transfer(this);

                    //================================================

                    // get & parse their data
                    foreach (var p in Player)
                    {
                        // get 
                        p.Connection.FlushInput();

                        // parse
                        foreach (var pack in _parser.Parse(p))
                            _dispatch.Handle(p, pack.Opcode, pack.Packet);
                    }

                    //================================================

                    // movement updates
                    var size = Movement.Count;
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
                    size = Player.Count;
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
            finally
            {
                IsRunning = false;
            }
        }

        public void Dispose()
        {
            IsRunning = false;
        }
    }
}