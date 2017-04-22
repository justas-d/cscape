using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape
{
    public interface IPlayerLogin
    {
        [CanBeNull]
        Player Transfer(EntityPool<Player> players);
    }
}