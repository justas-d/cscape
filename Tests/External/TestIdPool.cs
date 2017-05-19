using System.Collections.Generic;
using System.Linq;
using CScape.Basic.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CScape.Dev.Tests.External
{
    [TestClass]
    public class TestIdPool
    {
        [TestMethod]
        public void TestAlloc()
        {
            var p = new InternalIdPool();
            var used = new HashSet<uint>();
            const int iter = 1000000;

            for (var i = 0; i < iter; i++)
            {
                var id = p.NextId();
                Assert.IsFalse(used.Contains(id));
                used.Add(id);
            }
        }

        [TestMethod]
        public void TestAllocDealloc()
        {
            var p = new InternalIdPool();
            var used = new HashSet<uint>();
            const int iter = 1000000;

            for (var i = 0; i < iter; i++)
            {
                if (i % 2 != 0)
                {
                    var id = p.NextId();
                    Assert.IsFalse(used.Contains(id));
                    used.Add(id);
                }
                else
                {
                    if (used.Count == 0) continue;

                    var id = used.First();
                    p.FreeId(id);
                    used.Remove(id);
                }
            }
        }

        [TestMethod]
        public void TestAllocOutOfId()
        {
            const int lim = 2000;
            var p = new InternalIdPool(lim);

            for (var i = 0; i < lim + 1; i++)
            {
                if (i == lim)
                    Assert.ThrowsException<OutOfIdException>(() => p.NextId());
                else
                    p.NextId();
            }
        }

        [TestMethod]
        public void TestAllocAndDeallocOutOfId()
        {
            const int lim = 2000;
            var p = new InternalIdPool(lim);
            var expectedDealloc = new HashSet<uint>();

            var dealloc = 0;
            for (var i = 0; i < lim; i++)
            {
                var id = p.NextId();
                if (i % 2 != 0)
                {
                    dealloc++;
                    p.FreeId(id-1);
                    expectedDealloc.Add(id-1);
                }
            }

            for (var i = 0; i < dealloc; i++)
            {
                var id = p.NextId();
                Assert.IsTrue(expectedDealloc.Contains(id));
            }

            Assert.ThrowsException<OutOfIdException>(() => p.NextId());
        }
    }
}
