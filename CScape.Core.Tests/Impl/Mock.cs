using System;
using System.IO;
using System.Reflection;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Factory;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Game.Skill;
using CScape.Core.Json;
using CScape.Core.Network;
using CScape.Core.Network.Handler;
using CScape.Models;
using CScape.Models.Data;
using CScape.Models.Extensions;
using CScape.Models.Game;
using CScape.Models.Game.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core.Tests.Impl
{
    internal static class Mock
    {
        private static JsonPacketDatabase PacketDb { get; set; }

        public static void SpamTrash(this IPacketHandler h, IEntity ent)
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

                    h.Handle(ent, PacketMessage.Success((byte)op, b));
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

        public static void HandleAll(this IPacketHandler h, IEntity p, Func<int, PacketMessage> translate, Action action = null)
        {
            foreach (var op in h.Handles)
            {
                h.Handle(p, translate(op));
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

        public static readonly IPosition Invariant = new ImmIntVec3(3220, 3218, 0);

        public static MockServer Server(IServiceCollection c) => new MockServer(c);
        public static MockServer Server() => new MockServer();

        public static IEntityHandle Player(IGameServer server, IPosition pos) => Player("mock player", server, pos);
        public static IEntityHandle Player(IGameServer server) => Player("mock player", server);

        public static IEntityHandle Player(string name, IGameServer server, IPosition pos)
        {
            var players = server.Services.ThrowOrGet<PlayerFactory>();
            var p = players.Create(SerializablePlayerModel.Default(name, server.Services.ThrowOrGet<SkillDb>()), MockSocketContext.Instance, MockPacketParser.Instance, new PacketHandlerCatalogue(server.Services));
            p.Get().GetTransform().Teleport(pos);
            return p;
        }

        public static IEntityHandle Player(string name, IGameServer server) => Player(name, server, Invariant);

        public static IEntityHandle Npc(IGameServer server, IPosition pos, short id, string name)
        {
            var npcs = server.Services.ThrowOrGet<NpcFactory>();
            var p = npcs.Create(name, id);
            p.Get().GetTransform().Teleport(pos);
            return p;
        }
    }
}