using System;
using System.Diagnostics;
using System.Linq;
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
        [NotNull] private readonly Stopwatch _tickWatch = new Stopwatch();
        private readonly ILoginService _login;
        private readonly ILogger _log;
        private readonly IGameServerConfig _config;
        private readonly IPlayerDatabase _db;

        private int _waitTimeCarry;
        public IGameServer Server { get; }
        public long ElapsedMilliseconds => _tickWatch.ElapsedMilliseconds;

        public long DeltaTime { get; private set; }
        public long TickProcessTime { get; private set; }
        public int TickRate { get; set; }

        public bool IsRunning { get; private set; } = true;

        public MainLoop(IServiceProvider services)
        {
            Server = services.ThrowOrGet<IGameServer>();
            _log = services.ThrowOrGet<ILogger>();
            _login = services.ThrowOrGet<ILoginService>();
            _config = services.ThrowOrGet<IGameServerConfig>();
            _db = services.ThrowOrGet<IPlayerDatabase>();

            TickRate = _config.TickRate;
        }
        
        public async Task Run()
        {
            var timeSinceLastSave = 0L;
            _log.Normal(this, "Starting main loop...");

            // todo : exception handle all over the main loop
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

                foreach (var ent in Server.Entities.All.Values)
                    ent.DoTickUpdate(this);

                foreach (var ent in Server.Entities.All.Values)
                    ent.DoNetworkUpdate(this);

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
                    // don't do anything if player isn't connected
                    if (!p.Connection.IsConnected())
                        continue;

                    // write our data
                    foreach (var sync in p.Connection.SyncMachines)
                        sync.Synchronize(p.Connection.OutStream);

                    // send our data
                    p.Connection.FlushOutputStream();
                }

                //================================================

                void EntityUpdate<T>(IUpdateQueue<T> queue) where T : IWorldEntity
                {
                    size = queue.Count;
                    for (var i = 0; i < size; ++i)
                        queue.Dequeue().Update(this);
                }

                //================================================

                EntityUpdate(Player);

                //================================================

                EntityUpdate(Npc);

                //================================================

                EntityUpdate(Item);

                //================================================

                // handle tick delays
                TickProcessTime = _tickWatch.ElapsedMilliseconds;
                var waitTime = TickRate - Convert.ToInt32(TickProcessTime) + _waitTimeCarry;

                // tick process time took more then tickrate
                if (0 > waitTime)
                {
                    _waitTimeCarry = waitTime;
                    _log.Warning(this,
                        $"Cannot keep up! Tick rate is {TickRate}ms but wait time is {waitTime}ms which makes us carry {_waitTimeCarry}ms to next tick");
                }
                else // valid waitTime, wait it out
                {
                    _waitTimeCarry = 0;
                    await Task.Delay(waitTime);
                }

                DeltaTime = waitTime + TickProcessTime;
            }
            IsRunning = false;
        }


        public void Dispose()
        {
            IsRunning = false;
        }
    }
}