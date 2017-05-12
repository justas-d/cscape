using System;
using System.Diagnostics;
using CScape.Core.Game.Item;
using CScape.Core.Injection;
using Microsoft.Extensions.DependencyInjection;

namespace CScape.Core
{
    public static class ExtensionMethods
    {
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static T ThrowOrGet<T>(this IServiceProvider provider) where T : class
            => provider.GetService<T>() ?? throw new NullReferenceException(typeof(T).Name);

        internal static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            return val.CompareTo(max) > 0 ? max : val;
        }

        /// <summary>
        /// Returns the item definition for the given item id from the server db, asserting that returned item def id == given id and that the max amount value is in (0; int.MaxValue]
        /// </summary>
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
