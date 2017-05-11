using CScape.Core.Game.Entity;

namespace CScape.Core
{
    public interface IRegisteredCollection
    {
        bool Add(IWorldEntity obj);
        bool Remove(IWorldEntity obj);
    }
}