using CScape.Game.Entity;

namespace CScape
{
    public interface IRegisteredCollection
    {
        bool Add(IWorldEntity obj);
        bool Remove(IWorldEntity obj);
    }
}