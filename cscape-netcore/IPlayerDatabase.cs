using System.Threading.Tasks;
using JetBrains.Annotations;

namespace cscape
{
    public interface IPlayerDatabase
    {
        Task<bool> UserExists([NotNull] string username);
        Task<bool> IsValidPassword([NotNull] string pwdHash, [NotNull] string pwd);

        Task<IPlayerSaveData> Load([NotNull] string username, [NotNull] string password);
        Task<IPlayerSaveData> Save(Player player);
        Task<IPlayerSaveData> LoadOrCreateNew([NotNull] string username, [NotNull] string pwd);
    }
}