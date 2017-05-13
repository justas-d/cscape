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
        private readonly IGameServerConfig _config;
        private readonly IPlayerDatabase _db;

        private int _waitTimeCarry;
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
            _config = services.ThrowOrGet<IGameServerConfig>();
            _db = services.ThrowOrGet<IPlayerDatabase>();

            TickRate = _config.TickRate;
        }

        public bool IsRunning { get; private set; } = true;

        public async Task Run()
        {
            var timeSinceLastSave = 0L;
            _log.Normal(this, "Starting main loop...");

            // todo : exception handle all over the main loop
            try
            {
                while (IsRunning)
                {
                    /* Try autosave */
                    if ((timeSinceLastSave += DeltaTime) >= _config.AutoSaveIntervalMs)
                    {
                        _log.Debug(this, "Autosaving...");
                        await _db.Save();
                        timeSinceLastSave = 0;
                    }

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
                    var waitTime = TickRate - Convert.ToInt32(TickProcessTime) + _waitTimeCarry;
                    
                    // tick process time took more then tickrate
                    if (0 > waitTime)
                    {
                        _waitTimeCarry = waitTime;
                        _log.Warning(this, $"Cannot keep up! Tick rate is {TickRate}ms but wait time is {waitTime}ms which makes us carry {_waitTimeCarry}ms to next tick");
                    }
                    else // valid waitTime, wait it out
                    {
                        _waitTimeCarry = 0;
                        await Task.Delay(waitTime);
                    }

                    DeltaTime = waitTime + TickProcessTime;
                }
            }
            catch (Exception ex)
            {
                Debug.Fail($"Main loop crash {ex}");
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