using System;
using CScape.Models.Game.Entity.FacingData;

namespace CScape.Core.Game.Entities.FacingData
{
    public sealed class FacingCoordinate : IFacingData
    {
        public short SyncX { get; }
        public short SyncY { get; }
        public int RawX { get; }
        public int RawY { get; }

        public FacingCoordinate(int x, int y)
        {
            SyncX = Convert.ToInt16((x * 2) + 1);
            SyncY = Convert.ToInt16((y * 2) + 1);

            RawX = x;
            RawY = y;
        }
    }
}