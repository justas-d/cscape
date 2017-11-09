using System;
using System.Collections.Generic;

namespace CScape.Core.Utility
{
    public static class Algorithm
    {
        /// <summary>
        /// Performs a binary search of <see cref="targetValue"/> on an abstract set 
        /// of values retrieved by the <see cref="valueRetriever"/> method, yielding 
        /// a result each time it recurses.
        /// </summary>
        /// <param name="valueRetriever">The method which retrieves values from an abstract set.</param>
        /// <param name="targetValue">The value which we are attempting to find in side the set.</param>
        /// <param name="floorIdx">The lower bound of the search boundary. The algorithm will backpedal past this index.</param>
        /// <param name="roofIdx">The upper bound of the search boundary. The algorithm will step past this index.</param>
        public static IEnumerable<(int index, int value)> ProgressiveBinarySearch(
            Func<int, int> valueRetriever,
            int targetValue,
            int floorIdx, int roofIdx)
        {
            // recursion made iteration
            while (true)
            {
                // don't go out of range
                if (floorIdx > roofIdx)
                    break;

                var middleIdx = (floorIdx + roofIdx) / 2;
                var middleValue = valueRetriever(middleIdx);
                var result = (middleIdx, middleValue);

                if (middleValue == targetValue)
                {
                    yield return result;
                    break;
                }
                else if (middleValue < targetValue)
                {
                    floorIdx = middleIdx + 1;
                    yield return result;
                }
                else if (middleValue > targetValue)
                {
                    roofIdx = middleIdx - 1;
                    yield return result;
                }
            }
        }
    }
}
