using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CScape.Core.Extensions
{
    public static class MathExtensions
    {
        /// <summary>
        /// Checks if val is in range [begin, end)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InRange(this int val, int begin, int end)
        {
            return val >= begin && end > val;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short CastClamp(this int val, short min, short max)
        {
            if (val > max) return max;
            if (min > val) return min;
            return (short)val;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            return val.CompareTo(max) > 0 ? max : val;
        }
    }
}
