using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using CScape.Models.Data;

namespace CScape.Core
{
    public sealed class ServiceNotProvidedException : Exception
    {
        public Type ServiceType { get; }

        public ServiceNotProvidedException(Type serviceType) => ServiceType = serviceType;
        public override string ToString() => $"Service not provided: {ServiceType.Name}";
    }

    public static class Utils
    {
        public static string GetExeDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        /// <summary>
        /// Checks if val is in range [begin, end)
        /// </summary>
        public static bool InRange(this int val, int begin, int end)
        {
            return val >= begin && end > val;
        }

        //smh
        public static long StringToLong(string s)
        {
            var l = 0L;

            foreach (var c in s)
            {
                l *= 37L;
                if (c >= 'A' && c <= 'Z') l += 1 + c - 65;
                else if (c >= 'a' && c <= 'z') l += 1 + c - 97;
                else if (c >= '0' && c <= '9') l += 27 + c - 48;
            }

            while (l % 37L == 0L && l != 0L)
                l /= 37L;

            return l;
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        public static BlobPlaceholder Placeholder(this Blob blob, int size)
            => new BlobPlaceholder(blob, blob.WriteCaret, size);

        [DebuggerStepThrough]
        [DebuggerHidden]
        public static T ThrowOrGet<T>(this IServiceProvider provider) where T : class
            => provider.GetService<T>() ?? throw new ServiceNotProvidedException(typeof(T));

        internal static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            return val.CompareTo(max) > 0 ? max : val;
        }
    }
}
