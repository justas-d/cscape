using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network;
using CScape.Models;
using CScape.Models.Data;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;
using CScape.Models.Extensions;

namespace CScape.Core
{
    public sealed class MainLoop : IMainLoop, IDisposable
    {
        [NotNull] private readonly Stopwatch _tickWatch = new Stopwatch();

        private readonly Lazy<ILogger> _log;
        private readonly Lazy<IEntitySystem> _system;
        private readonly Lazy<IGameServer> _server;
        private readonly Lazy<int> _gcIntervalMs;
        private readonly Lazy<SocketAndPlayerDatabaseDispatch> _dispatch;

        public IGameServer Server => _server.Value;
        public ILogger Log => _log.Value;
        public IEntitySystem EntSystem => _system.Value;
        public IConfigurationService Config { get; }
        public SocketAndPlayerDatabaseDispatch Dispatch => _dispatch.Value;

        private long DeltaTime { get; set; }
        public long TickProcessTime { get; private set; }
        public int TickRate { get; set; }

        public bool IsRunning { get; private set; } = true;
        public int GcIntervalMs => _gcIntervalMs.Value;

        private long _timeSinceGc = 0L;

        public MainLoop([NotNull] IServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            _log = services.GetLazy<ILogger>();
            _system = services.GetLazy<IEntitySystem>();
            _server = services.GetLazy<IGameServer>();
            _dispatch = services.GetLazy<SocketAndPlayerDatabaseDispatch>();
            Config = services.ThrowOrGet<IConfigurationService>();

            TickRate = Config.GetInt(ConfigKey.TickRate);
            _gcIntervalMs = Config.GetLazy(ConfigKey.EntityGcInternalMs, int.Parse);
        }

        public long GetDeltaTime() => DeltaTime + _tickWatch.ElapsedMilliseconds; // last frame + how much into this frame

        public async Task Run(CancellationToken token)
        {
            Log.Normal(this, "Main loop is live.");

            // todo : exception handle all over the main loop
            while (IsRunning)
            {
                try
                {
                    if (token.IsCancellationRequested)
                        HandleCancellation();

                    await RunTimedGameTick(token);
                }
                catch (TaskCanceledException)
                {
                    HandleCancellation();
                }
            }
        }

        private async Task RunTimedGameTick(CancellationToken token)
        {
            BeginFrameClock();

            SimulateGame();

            var waitTime = EndFrameClock();
            await Wait(waitTime, token);
        }

        private int EndFrameClock()
        {
            // todo : tick process time kind of bleeds into DT such that DT > TicKRate. investigate
            // handle tick delays
            TickProcessTime = _tickWatch.ElapsedMilliseconds;
            var waitTime = TickRate - Convert.ToInt32(TickProcessTime);
            
            DeltaTime = waitTime + TickProcessTime;

            return waitTime;
        }

        private async Task Wait(int time, CancellationToken token)
        {
            if (0 > time)
            {
                Log.Warning(this,
                    $"Cannot keep up! Tick rate is {TickRate}ms but tick took {TickProcessTime}ms. Wait time is {time}ms.");
                return;
            }

            await Task.Delay(time, token).ConfigureAwait(false);
        }

        private void BeginFrameClock()
        {
            _tickWatch.Restart();
        }

        private void HandleCancellation()
        {
            IsRunning = false;
            Log.Normal(this, "MainLoop was signaled to cancel by its cancellation token.");
        }

        private void SimulateGame()
        {
            PerformGc();
            HandleLogins();            
            StimulateEntities();

            EntSystem.PostFrame();
        }

        private void StimulateEntities()
        {
            SendMessage(NotificationMessage.FrameBegin);
            SendMessage(NotificationMessage.NetworkPrepare);
            SendMessage(NotificationMessage.NetworkSync);
            SendMessage(NotificationMessage.FrameEnd);
        }

        private void HandleLogins()
        {
            IPlayerLogin next;
            while ((next = Dispatch.TryGetNext()) != null)
                next.Transfer(this);
        }

        private void PerformGc()
        {
            if ((_timeSinceGc += DeltaTime) >= GcIntervalMs)
            {
                Log.Normal(this, "Sending Entity GC message");

                SendMessage(NotificationMessage.GC);

                Log.Normal(this, "Performing world GC");
                Server.Overworld.GC();

                // TODO : PoE factory, iterate over all PoE's when it's time for entity GC
                _timeSinceGc = 0;
            }
        }

        private void SendMessage(IGameMessage msg)
        {
            foreach (var ent in EntSystem.All.Values)
                ent.SendMessage(msg);
        }

        public void Dispose()
        {
            IsRunning = false;
        }
    }
}