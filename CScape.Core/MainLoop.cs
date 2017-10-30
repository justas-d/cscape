using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network;
using CScape.Models;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core
{
    public sealed class MainLoop : IMainLoop, IDisposable
    {
        [NotNull] private readonly Stopwatch _tickWatch = new Stopwatch();

        private readonly ILogger _log;
        private readonly IGameServerConfig _config;
        private readonly IEntitySystem _sys;
        private readonly SocketAndPlayerDatabaseDispatch _dispatch;

        public IGameServer Server { get; }

        private long DeltaTime { get; set; }
        public long TickProcessTime { get; private set; }
        public int TickRate { get; set; }

        public bool IsRunning { get; private set; } = true;

        public MainLoop(IServiceProvider services)
        {
            Server = services.ThrowOrGet<IGameServer>();
            _log = services.ThrowOrGet<ILogger>();
            _config = services.ThrowOrGet<IGameServerConfig>();
            _sys = services.ThrowOrGet<IEntitySystem>();
            _dispatch = services.ThrowOrGet<SocketAndPlayerDatabaseDispatch>();
        
            TickRate = _config.TickRate;
        }

        public long GetDeltaTime() => DeltaTime + _tickWatch.ElapsedMilliseconds; // last frame + how much into this frame

        public async Task Run(CancellationToken token)
        {
            var timeSinceGc = 0L;

            _log.Normal(this, "Main loop is live.");

            // todo : exception handle all over the main loop
            while (IsRunning)
            {
                try
                {
                    _tickWatch.Restart();

                    void SendMessage(IGameMessage msg)
                    {
                        foreach (var ent in _sys.All.Values)
                            ent.SendMessage(msg);
                    }

                    /* Entity gc */
                    if ((timeSinceGc += DeltaTime) >= _config.EntityGcInternalMs)
                    {
                        _log.Normal(this, "Sending Entity GC message");
                        SendMessage(NotificationMessage.GC);
                        _log.Normal(this, "Performing world GC");
                        Server.Overworld.GC();
                        // TODO : PoE factory, iterate over all PoE's when it's time for entity GC
                        timeSinceGc = 0;
                    }

                    //================================================

                    // handle new logins
                    IPlayerLogin next;
                    while ((next = _dispatch.TryGetNext()) != null)
                        next.Transfer(this);

                    //================================================

                    SendMessage(NotificationMessage.FrameUpdate);
                    SendMessage(NotificationMessage.NetworkPrepare);
                    SendMessage(NotificationMessage.NetworkSync);
                    SendMessage(NotificationMessage.FrameEnd);

                    //================================================

                    _sys.PostFrame();

                    // handle tick delays
                    TickProcessTime = _tickWatch.ElapsedMilliseconds;
                    var waitTime = TickRate - Convert.ToInt32(TickProcessTime);

                    // tick process time took more then tickrate
                    if (0 > waitTime)
                    {
                        waitTime = 0;
                        _log.Warning(this,
                            $"Cannot keep up! Tick rate is {TickRate}ms but wait time is {waitTime}ms.");
                    }
                    else // valid waitTime, wait it out
                    {
                        await Task.Delay(waitTime, token);
                    }

                    DeltaTime = waitTime + TickProcessTime;

                    // check for cancellation
                    if (token.IsCancellationRequested)
                    {
                        IsRunning = false;
                        _log.Normal(this, "MainLoop was signaled to cancel by its cancellation token.");
                    }
                }
                catch (TaskCanceledException)
                {
                    // expected
                    IsRunning = false;
                    _log.Normal(this, "MainLoop caught TaskCancelledException, bailing.");
                }
            }
            IsRunning = false;
        }

        public void Dispose()
        {
            IsRunning = false;
        }
    }
}