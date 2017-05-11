using System;
using System.Diagnostics;
using CScape.Game.Item;

namespace CScape
{
    internal static class ExtensionMethods
    {
        internal static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            return val.CompareTo(max) > 0 ? max : val;
        }

        /// <summary>
        /// Returns the item definition for the given item id from the server db, asserting that returned item def id == given id.
        /// </summary>
        internal static IItemDefinition GetAsserted(this GameServer server, int id)
        {
            var item = server.Database.Item.Get(id);
            if (item == null)
                return null;

            if (item.ItemId != id)
            {
                server.Log.Warning(server.Database.Item, $"returned item id ({item.ItemId})!= given item id ({id})");
#if RELEASE
                return null;
#endif
            }

            Debug.Assert(id == item.ItemId);
            return item;
        }
    }
}
