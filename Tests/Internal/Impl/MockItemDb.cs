using CScape.Basic.Model;
using CScape.Core.Game.Item;
using CScape.Core.Injection;

namespace CScape.Dev.Tests.Internal.Impl
{
    public class MockItemDb : IItemDefinitionDatabase
    {
        public IItemDefinition Get(int id) => new TestItem(id, "Mock", int.MaxValue, true, 1, false, -1);
    }
}