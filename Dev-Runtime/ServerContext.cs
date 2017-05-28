using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using CScape.Basic;
using CScape.Basic.Commands;
using CScape.Basic.Database;
using CScape.Basic.Model;
using CScape.Basic.Server;
using CScape.Core;
using CScape.Core.Injection;
using CScape.Core.Network;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using Utils = CScape.Basic.Server.Utils;

namespace CScape.Dev.Runtime
{
    public class ServerContext
    {
        public GameServer Server { get; private set; }

        private void HandleAggregateException(AggregateException aggEx)
        {
            foreach (var ex in aggEx.InnerExceptions)
                ExceptionDispatchInfo.Capture(ex).Throw();
            // Enable all CLR exceptions in the exception settings window to see the stack-trace.
        }

        public void RunBlocking()
        {
            // make sure we're invariant
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var dirBuild = Path.GetDirectoryName(GetType().GetTypeInfo().Assembly.Location);

            // make sure we get notified of unobserved task exceptions
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                if (e.Observed)
                    return;
                HandleAggregateException(e.Exception);
                e.SetObserved();
            };
            
            // set up servicess
            var services = new ServiceCollection();

            services.AddSingleton<ILogger>(s => new Logger(s.ThrowOrGet<IGameServer>()));
            services.AddSingleton<IMainLoop>(s => new MainLoop(s));
            services.AddSingleton<ILoginService>(s => new SocketAndPlayerDatabaseDispatch(s.ThrowOrGet<IGameServer>().Services));
            services.AddSingleton<IPacketParser>(s => new PacketParser(s.ThrowOrGet<IGameServer>().Services));

            services.AddSingleton<IPlayerDatabase>(s =>
            {
                var db = new PlayerDatabase();
                db.Database.EnsureCreated();
                return db;
            });

            var cfg = JsonConvert.DeserializeObject<JsonGameServerConfig>(
                File.ReadAllText(
                    Path.Combine(dirBuild, "config.json")));

            services.AddSingleton<IItemDefinitionDatabase>(s => new ItemDefinitionDatabase());
            services.AddSingleton<IPacketDispatch>(s => new PacketDispatch(s));
            services.AddSingleton<IPacketParser>(s => new PacketParser(s));
            services.AddSingleton<IIdPool>(s => new IdPool());
            services.AddSingleton<ICommandHandler>(s => new CommandDispatch());

            services.AddSingleton<IInterfaceIdDatabase>(
                s => InterfaceDb.FromJson(Path.Combine(dirBuild, "interface-ids.json")));

            services.AddSingleton<IGameServerConfig>(s => cfg);

            services.AddSingleton<IPacketDatabase>(s => 
                JsonConvert.DeserializeObject<JsonPacketDatabase>(
                        File.ReadAllText(
                            Path.Combine(dirBuild, "packet-lengths.json"))));


            Server = new GameServer(services);

            var crypto = Utils.GetCrypto(cfg, true);
            var idx = 0;

            // dispatch a thread to run the server

            AsyncContext.Run(async () =>
            {
                Task.Run(async () => await Server.Start());


                while (true)
                {
                    Console.ReadLine();
                    var p = new FakePlayer((short) cfg.Revision, cfg.ListenEndPoint, crypto, $"Fake_{idx++}", "1");
                    Console.WriteLine($"Spawning {p.Username}");

                    await p.Login();
                }
            });
        }
    }
}