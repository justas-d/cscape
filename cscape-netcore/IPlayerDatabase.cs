using System.Threading.Tasks;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape
{
    public interface IPlayerDatabase
    {
        [CanBeNull] Task<PlayerModel> GetPlayer(string username);
        [CanBeNull] Task<PlayerModel> GetPlayer(string username, string password);
        Task Save();
        [CanBeNull] Task<PlayerModel> CreatePlayer(string username, string password);
        Task<bool> IsValidPassword(string pwd1, string pwd2);
    }
}