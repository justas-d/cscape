using CScape.Game.Interface;
using CScape.Game.Item;
using JetBrains.Annotations;

namespace CScape
{
    public interface IItemDatabase
    {
        /// <summary>
        /// Returns the item definition of the provided id.
        /// Implementation must guarantee that returned IItemDefinition.Id == id
        /// </summary>
        /// <returns></returns>
        [CanBeNull] IItemDefinition Get(int id);
    }
}