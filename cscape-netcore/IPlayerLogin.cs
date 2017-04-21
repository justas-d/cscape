using CScape.Game.Entity;

namespace CScape
{
    public interface IPlayerLogin
    {
        void Transfer(EntityPool<Player> players);
    }
}