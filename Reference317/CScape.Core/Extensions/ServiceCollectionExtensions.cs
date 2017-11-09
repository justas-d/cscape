using System;
using System.IO;
using CScape.Core.Game.Skill;
using CScape.Core.Json;
using CScape.Core.Json.Model;
using CScape.Core.Network;
using CScape.Models.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CScape.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void WithSkillDb(this IServiceCollection builder)
        {
            builder.AddSingleton(s => new SkillDb(s));
        }

        public static void WithInterfaceDatabase(this IServiceCollection builder, string dir)
        {
            builder.AddSingleton(s => InterfaceIdDatabase.FromJson(dir));
            builder.AddSingleton<IInterfaceIdDatabase>(s => s.ThrowOrGet<InterfaceIdDatabase>());

        }

        public static void WithItemDatabase(this IServiceCollection builder)
        {
            builder.AddSingleton(s => new ItemDatabase());
        }

        public static void WithPacketDatabase(this IServiceCollection builder, string dir)
        {
            builder.AddSingleton<IPacketDatabase>(s =>
                JsonConvert.DeserializeObject<JsonPacketDatabase>(
                    File.ReadAllText(dir)));
        }

        public static void WithPacketParser(this IServiceCollection builder)
        {
            builder.AddSingleton<IPacketParser>(s => new PacketParser(s));
        }

        public static void WithPacketHandlerCatalogue(this IServiceCollection builder)
        {
            builder.AddSingleton<IPacketHandlerCatalogue>(s => new PacketHandlerCatalogue(s));
        }

        public static void WithPlayerDatabase(this IServiceCollection builder)
        {
            builder.AddSingleton<PlayerJsonDatabase>(s => new PlayerJsonDatabase(s));
        }
    }
}
