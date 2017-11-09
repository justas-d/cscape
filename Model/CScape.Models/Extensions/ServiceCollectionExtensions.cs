using System;
using CScape.Models.Data;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Item;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Models.Extensions
{

    public static class ServiceCollectionExtensions
    {

        public static void DoublyMappedSingleton<TReal, TMapped>(this IServiceCollection builder, Func<IServiceProvider, TReal> factory)
            where TMapped : class
            where TReal : class, TMapped
        {
            builder.AddSingleton(factory);
            builder.AddSingleton<TMapped>(s => s.ThrowOrGet<TReal>());
        }

        public static void WithPlayerCatalogue<T>(this IServiceCollection builder, Func<IServiceProvider, T> factory)
            where T : class, IPlayerCatalogue
        {
            //TODO: player factory
            builder.DoublyMappedSingleton<T, IPlayerCatalogue>(factory);
        }

        public static void WithNpcFactory<T>(this IServiceCollection builder, Func<IServiceProvider, T> factory)
            where T : class, INpcFactory
        {
            builder.DoublyMappedSingleton<T, INpcFactory>(factory);
        }

        public static void WithMainLoop<T>(this IServiceCollection builder, Func<IServiceProvider, T> factory)
            where T : class, IMainLoop
        {
            builder.DoublyMappedSingleton<T, IMainLoop>(factory);
        }

        public static void WithGroundItemFactory<T>(this IServiceCollection builder, Func<IServiceProvider, T> factory)
            where T : class, IGroundItemFactory
        {
            builder.DoublyMappedSingleton<T, IGroundItemFactory>(factory);
        }

        public static void WithEntitySystem<T>(this IServiceCollection builder, Func<IServiceProvider, T> factory)
            where T : class, IEntitySystem
        {
            builder.DoublyMappedSingleton<T, IEntitySystem>(factory);
        }

        public static void WithLogger<T>(this IServiceCollection builder, Func<IServiceProvider, T> factory)
            where T : class, ILogger
        {
            builder.DoublyMappedSingleton<T, ILogger>(factory);
        }

        public static void WithConfigurationService<T>(this IServiceCollection builder, Func<IServiceProvider, T> factory)
            where T : class, IConfigurationService
        {
            builder.DoublyMappedSingleton<T, IConfigurationService>(factory);   
        }
    }
}