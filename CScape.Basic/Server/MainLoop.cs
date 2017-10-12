using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CScape.Core;
using CScape.Core.Game.Entities;
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
            var timeSinceGc = 0L;

            _log.Normal(this, "Starting main loop...");

            // todo : exception handle all over the main loop
            while (IsRunning)
            {
                _tickWatch.Restart();

                void SendMessage(GameMessage msg)
                {
                    foreach (var ent in Server.Entities.All.Values)
                        ent.SendMessage(msg);
                }

                /* Try autosave */
                if ((timeSinceLastSave += DeltaTime) >= _config.AutoSaveIntervalMs)
                {
                    _log.Normal(this, "Autosaving...");
                    await _db.Save();
                    timeSinceLastSave = 0;
                }

                /* Entity gc */
                if ((timeSinceGc += DeltaTime) >= _config.AutoSaveIntervalMs)
                {
                    _log.Normal(this, "Sending Entity GC message");
                    SendMessage(GameMessage.GC);
                    _log.Normal(this, "Performing world GC");
                    Server.Overworld.GC();
                    // TODO : PoE factory, iterate over all PoE's when it's time for entity GC
                    timeSinceGc = 0;
                }

                //================================================

                // handle new logins
                IPlayerLogin next;
                while ((next = _login.TryGetNext()) != null)
                    next.Transfer(this);

                //================================================


                SendMessage(GameMessage.FrameUpdate);
                SendMessage(GameMessage.DatabaseUpdate);
                SendMessage(GameMessage.NetworkUpdate);
                SendMessage(GameMessage.FrameEnd);

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