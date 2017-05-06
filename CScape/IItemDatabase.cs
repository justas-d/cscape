using CScape.Game.Interface;
using JetBrains.Annotations;

namespace CScape
{
    public interface IItemDatabase
    {
        [CanBeNull] IItemDefinition Get(int id);
    }
}