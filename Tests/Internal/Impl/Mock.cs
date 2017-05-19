using System;
using CScape.Basic.Model;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Dev.Tests.Internal.Impl
{
    internal static class Mock
    {
        public static readonly IPosition Invariant = new Position(3220, 3218, 0);

        public static MockServer Server(IServiceCollection c) => new MockServer(c);
        public static MockServer Server() => new MockServer();

        public static Player Player(string name, IGameServer server, IPosition pos)
        {
            var model = new PlayerModel(name, "1");
            model.SyncPosition(pos);
            return new Player(model, new MockSocketContext(), server.Services, true);
        }

        public static Player Player(string name, IGameServer server) => Player(name, server, Invariant);

        public static Npc Npc(IGameServer server, short id) => new Npc(server.Services, id, Invariant);
    }
}