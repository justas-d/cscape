using System.Collections.Generic;

namespace cscape
{
    public class World
    {
        // don't want to deal with tuples
        // { x, {y, region}}
        private readonly Dictionary<int, Dictionary<int, Region>> _regions = new Dictionary<int, Dictionary<int, Region>>();

        public Region GetRegion(ushort x, ushort y)
        {
            if(!_regions.ContainsKey(x))
                _regions[x] = new Dictionary<int, Region>();

            var yDict = _regions[x];
            if(!yDict.ContainsKey(y))
                yDict[y] = new Region(x,y);

            return yDict[y];
        }
    }
}