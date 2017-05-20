using System;
using System.IO;
using System.Reflection;
using CScape.Basic.Database;
using CScape.Basic.Model;
using CScape.Core;
using CScape.Core.Data;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using CScape.Core.Injection;
using CScape.Core.Network;
using CScape.Core.Network.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.Internal.Impl
{
    internal static class Mock
    {
        public static (MockItem, int amount, int idx) SetItem(
            MockServer s,
            IContainerInterface interf,
            int id, int amount, int idx)
        {
            var provider = interf.Items.Provider;
            provider.SetId(idx, id);
            provider.SetAmount(idx, amount);
            return (
                s.Services.ThrowOrGet<IItemDefinitionDatabase>().Get(id) as MockItem,
                amount,
                idx);
        }

        public static IContainerInterface Backpack(Player p) => GetContainer(p, 3214) as IContainerInterface;
        public static IContainerInterface Equipment(Player p) => GetContainer(p, 1688) as IContainerInterface;
        public static IBaseInterface NormalInterface(Player p) => GetContainer(p, 3917); // skills

        public static IBaseInterface GetContainer(Player p, int id)
        {
            var ret = p.Interfaces.TryGetById(id);
            Assert.IsNotNull(ret);
            return ret;
        }

        private static JsonPacketDatabase PacketDb { get; set; }

        public static void SpamTrash(this IPacketHandler h)
        {
            const int defaultShortSize = 1024; // size used for short size opcodes

            var s = Server();
            var p = Player(s);

            void Spam(int size, bool randomSize, int op)
            {
                var iterations = (int)Math.Pow(2, 13);

                for (var i = 0; i < iterations; i++)
                {
                    Blob b = randomSize
                        ? RandomData(Rng.Next(0, size + 1))
                        : RandomData(size);

                    h.Handle(p, op, b);
                }
            }

            if (PacketDb == null)
            {
                var dirBuild = Path.GetDirectoryName(typeof(MockServer).GetTypeInfo().Assembly.Location);
                PacketDb = new JsonPacketDatabase(Path.Combine(dirBuild, "packet-lengths.json"));
            }

            foreach (var op in h.Handles)
            {
                // chose size    
                var size = PacketDb.GetIncoming(op);
                switch (size)
                {
                    case PacketLength.NextByte:
                        Spam(byte.MaxValue, true, op);
                        break;
                    case PacketLength.NextShort:
                        Spam(defaultShortSize, true, op);
                        break;
                    case PacketLength.Undefined: throw new NotSupportedException($"Undefined packet in {h.GetType().Name}");
                    default:
                        var finalSize = (int) size;
                        if (finalSize == 0)
                            return;

                        Spam(finalSize, false, op);
                        break;
                }
            }
        }

        public static void HandleAll(this IPacketHandler h, Player p, Blob b, Action action = null)
        {
            foreach (var op in h.Handles)
            {
                b.ResetHeads();
                h.Handle(p, op, b);
                action?.Invoke();
            }
        }

        public static readonly Random Rng = new Random();

        public static Blob RandomData(int size)
        {
            var b = new Blob(size);
            Rng.NextBytes(b.Buffer);
            return b;
        }

        public static readonly IPosition Invariant = new Position(3220, 3218, 0);

        public static MockServer Server(IServiceCollection c) => new MockServer(c);
        public static MockServer Server() => new MockServer();

        public static Player Player(IGameServer server, IPosition pos) => Player("mock player", server, pos);
        public static Player Player(IGameServer server) => Player("mock player", server);

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