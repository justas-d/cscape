using System;
using CScape.Game.World;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace tests
{
    [TestClass]
    public class MiscTests
    {
        [TestMethod]
        public void TestDirectionHelper()
        {
            void Test(Direction dir)
            {
                Assert.IsTrue(DirectionHelper.GetDirection(DirectionHelper.GetDelta(dir)) == dir);
            }

            foreach (var t in Enum.GetValues(typeof(Direction)))
                Test((Direction)t);
        }

        [TestMethod]
        public void TestDirectionHelperGetDirectionException()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => DirectionHelper.GetDirection((-2, 2)));
        }
    }
}
