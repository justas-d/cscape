using System;
using System.Linq;
using CScape.Models.Game.World;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.ModelTests
{
    [TestClass]
    public class DirectionDeltaTests
    {
        [TestMethod]
        public void TestCtorConsistency()
        {
            // DirectionDelta(dir) == DirectionDelta(x y)
            foreach (var dir in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                var dirFromDir = new DirectionDelta(dir);
                var dirFromDelta = new DirectionDelta(dirFromDir.X, dirFromDir.Y);

                Assert.Equals(dirFromDir.Direction, dir);
                Assert.Equals(dirFromDelta.Direction, dir);

                Assert.Equals(dirFromDir.X, dirFromDelta.X);
                Assert.Equals(dirFromDir.Y, dirFromDelta.Y);
            }
        }

        [TestMethod]
        public void TestCtorOutOfRangeException()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new DirectionDelta(-2, 2));

            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new DirectionDelta((Direction)100));
        }
    }
}
