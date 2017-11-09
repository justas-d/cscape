using System;
using System.Linq;
using CScape.Models.Game.World;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Models.Tests
{
    [TestClass]
    public class DirectionDeltaTests
    {
        [TestMethod]
        public void TestCtorConsistency()
        {
            foreach (var dir in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                var dirFromDir = new DirectionDelta(dir);
                var dirFromDelta = new DirectionDelta(dirFromDir.X, dirFromDir.Y);

                Assert.AreEqual(dirFromDir.Direction, dir);
                Assert.AreEqual(dirFromDelta.Direction, dir);

                Assert.AreEqual(dirFromDir.X, dirFromDelta.X);
                Assert.AreEqual(dirFromDir.Y, dirFromDelta.Y);
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
