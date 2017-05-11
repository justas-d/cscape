using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using CScape.Core;
using CScape.Core.Game.Entity;
using CScape.Core.Game.World;
using CScape.Core.Injection;
using CScape.Core.Network;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Basic.Server
{
    public sealed class GameServer : IGameServer
    {
        private ImmutableDictionary<int, Player> _players = ImmutableDictionary<int, Player>.Empty;
        
        public AggregateEntityPool<IWorldEntity> Entities { get; } = new AggregateEntityPool<IWorldEntity>();
        public IReadOnlyDictionary<int, Player> Players => _players;
        public IServiceProvider Services { get; }
        public PlaneOfExistance Overworld { get; }

        public bool IsDisposed { get; private set; }
        public DateTime StartTime { get; private set; }

        private ILogger Log { get; }
        private IMainLoop Loop { get; }

        public GameServer([NotNull] IServiceCollection services)
        {
            // todo : setup internal services
            // todo : assert paramater services

            // register internal dependencies
            services.AddSingleton<IGameServer>(_ => this);

            // build service provider
            Services = services?.BuildServiceProvider() ?? throw new ArgumentNullException(nameof(services));

            Loop = Services.ThrowOrGet<IMainLoop>();
            Log = Services.ThrowOrGet<ILogger>();

            /*
            Services.ThrowOrGet<IGameServerConfig>();
            Services.ThrowOrGet<IPlayerIdPool>();
            Services.ThrowOrGet<IPacketDatabase>();
            Services.ThrowOrGet<IEntityIdPool>();
            Services.ThrowOrGet<ILoginService>();
            Services.ThrowOrGet<IPacketParser>();
            */

            // init
            Overworld = new PlaneOfExistance(this, "Overworld");
        }

        public ServerStateFlags GetState()
        {
            ServerStateFlags ret = 0;

            if (Players.Count >= Services.GetService<IGameServerConfig>().MaxPlayers)
                ret |= ServerStateFlags.PlayersFull;

            if (!Services.GetService<ILoginService>().IsEnabled)
                ret |= ServerStateFlags.LoginDisabled;

            return ret;
        }

        public async Task Start()
        {
            StartTime = DateTime.Now;

            Log.Normal(this, "Starting server...");

            await Loop.Run();
        }

        public void RegisterPlayer(Player player)
        {
            Log.Debug(this, $"Registering new player: {player.Username}.");

            if (Players.ContainsKey(player.Pid))
            {
                Log.Warning(this, $"Tried to register existing player: {player.Username}.");
                return;
            }

            _players = _players.Add(player.Pid, player);
        }

        public void UnregisterPlayer(Player player)
        {
            Log.Debug(this, $"Unregistering player: {player.Username}.");

            if (!Players.ContainsKey(player.Pid))
            {
                Log.Warning(this, $"Tried to unregister player that is not registered: {player.Username}");
                return;
            }

            _players = _players.Remove(player.Pid);
        }

        public Player GetPlayerByPid(int pid)
        {
            if (!Players.ContainsKey(pid))
            {
                Log.Warning(this, $"Attempted to get unregistered pid: {pid}");
                return null;
            }

            return Players[pid];
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                // block as we're saving.
                Services.GetService<IPlayerDatabase>().Save().GetAwaiter().GetResult();

                foreach (var p in Players.Values)
                    p.Connection.Dispose();

                // todo : make sure we actually dispose Services.
                (Services as IDisposable)?.Dispose();

                IsDisposed = true;
            }
        }
    }
}
