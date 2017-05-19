using System;
using System.Diagnostics;
using CScape.Core.Data;
using CScape.Core.Game.Item;
using CScape.Core.Injection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

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
        public static DisposableBlobPlaceholder Placeholder(this Blob blob, int size)
            => new DisposableBlobPlaceholder(blob, blob.WriteCaret, size);

        [DebuggerStepThrough]
        [DebuggerHidden]
        public static T ThrowOrGet<T>(this IServiceProvider provider) where T : class
            => provider.GetService<T>() ?? throw new ServiceNotProvidedException(typeof(T));

        internal static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            return val.CompareTo(max) > 0 ? max : val;
        }

        /// <summary>
        /// Returns the item definition for the given item id from the server db, asserting that returned item def id == given id and that the max amount value is in (0; int.MaxValue]
        /// </summary>
        [CanBeNull]
        internal static IItemDefinition GetAsserted(this IItemDefinitionDatabase db, int id)
        {
            var item = db.Get(id);
            if (item == null)
                return null;

#if RELEASE
            if(id != item.ItemId) throw new InvalidOperationException("id != item.ItemId");
            if(0 >= item.MaxAmount && item.MaxAmount > int.MaxValue);throw new InvalidOperationException("0 >= item.MaxAmount && item.MaxAmount > int.MaxValue");
 
#else
            Debug.Assert(id == item.ItemId);
            Debug.Assert(0 < item.MaxAmount && item.MaxAmount <= int.MaxValue);
#endif

            return item;
        }
    }
}
