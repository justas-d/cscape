using System;
using System.Threading;
using System.Threading.Tasks;
using CScape.Models;

namespace CScape.Core.Tests.Impl
{
    public class MockLoop : IMainLoop
    {
        private readonly Random _rng = new Random();
        public MockLoop(IGameServer server, int tickRate = 600)
        {
            Server = server;
            TickRate = tickRate;
        }

        public IGameServer Server { get; }
        public long TickProcessTime => _rng.Next(0, (int)TickRate);
        public int TickRate { get; set; }
        public bool IsRunning => true;

        public long GetDeltaTime() => _rng.Next(0, (int)TickRate);

        public Task Run(CancellationToken token) => null;
    }
}