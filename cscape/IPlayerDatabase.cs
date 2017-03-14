using System.Threading.Tasks;

namespace cscape
{
    public interface IPlayerDatabase
    {
        Task<bool> UserExists(string username);
        Task<IPlayerSaveData> Load(string username, string password);
        Task Save(Player player);
        Task<IPlayerSaveData> LoadOrCreateNew(string username, string pwd);
        Task<bool> IsValidPassword(string pwdHash, string pwd);
    }
}