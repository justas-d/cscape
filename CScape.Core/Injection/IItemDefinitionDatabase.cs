using CScape.Core.Game.Item;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IItemDefinitionDatabase
    {
        /// <summary>
        /// Returns the item definition of the provided id.
        /// Implementation must guarantee that returned IItemDefinition.Id == id
        /// </summary>
        /// <returns></returns>
        [CanBeNull] IItemDefinition Get(int id);
    }
}