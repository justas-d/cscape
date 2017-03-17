using System.Threading.Tasks;

namespace cscape
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