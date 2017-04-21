using System.Threading.Tasks;
using CScape.Game.Entity;

namespace CScape
{
    public interface IPlayerDatabase
    {
        Task<bool> UserExists(string username);
        Task<bool> IsValidPassword(string pwdHash, string pwd);

        Task<IPlayerSaveData> Load(string username, string password);
        Task<IPlayerSaveData> Save(Player player);
        Task<IPlayerSaveData> LoadOrCreateNew(string username, string pwd);
    }
}