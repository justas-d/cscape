using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CScape.Models.Data;

namespace CScape.Models.Extensions
{
    public static class ConfigurationServiceExtensions
    {
        [DebuggerStepThrough]
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInt(this IConfigurationService config, string key)
        {
            return int.Parse(config.Get(key));
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Lazy<string> GetLazy(this IConfigurationService config, string key)
        {
            return new Lazy<string>(() => config.Get(key));
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Lazy<T> GetLazy<T>(this IConfigurationService config, string key, Func<string, T> converter)
        {
            return new Lazy<T>(() => converter(config.Get(key)));
        }
    }
}
