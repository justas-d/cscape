using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.External
{
    [TestClass]
    public class DirectionTests
    {
        [TestMethod]
        public void TestDirectionHelper()
        {
            void Test(Direction dir)
            {
                Assert.IsTrue(DirectionHelper.GetDirection(DirectionHelper.GetDelta(dir)) == dir);
            }

            foreach (var t in Enum.GetValues(typeof(Direction)))
                Test((Direction) t);
        }

        [TestMethod]
        public void TestDirectionHelperGetDirectionException()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => DirectionHelper.GetDirection((-2, 2)));
        }

        [TestMethod]
        public void TestDirectionInvertDir()
        {
            void Test(Direction dir)
            {
                Assert.IsTrue(DirectionHelper.Invert(DirectionHelper.Invert(dir)) == dir);
            }

            foreach (var t in Enum.GetValues(typeof(Direction)))
                Test((Direction) t);
        }

        [TestMethod]
        public void TestDirectionInvertThrow()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => DirectionHelper.Invert((Direction) 9));
        }

        [TestMethod]
        public void TestDirectionInvertDelta()
        {
            void Test(Direction dir)
            {
                var expected = DirectionHelper.Invert(dir);
                var expectedDelta = DirectionHelper.GetDelta(expected);
                var delta = DirectionHelper.Invert(DirectionHelper.GetDelta(dir));
                Assert.IsTrue(delta.x == expectedDelta.x && delta.y == expectedDelta.y);
                Assert.IsTrue(DirectionHelper.GetDirection(delta) == expected);
            }

            foreach (var t in Enum.GetValues(typeof(Direction)))
                Test((Direction)t);
        }
    }
}
