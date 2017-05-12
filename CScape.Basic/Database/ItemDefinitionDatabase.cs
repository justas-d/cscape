using CScape.Basic.Model;
using CScape.Core.Game.Item;
using CScape.Core.Injection;

namespace CScape.Basic.Database
{
    public sealed class ItemDefinitionDatabase : IItemDefinitionDatabase
    {
        public IItemDefinition Get(int id)
        {
            return new BasicItem(id, "Dummy", int.MaxValue, true, 1, false, -1);
        }
    }
}