using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IPlayerDatabase
    {
        [CanBeNull] Task<IPlayerModel> GetPlayer(string username);
        [CanBeNull] Task<IPlayerModel> GetPlayer(string username, string password);
        Task Save();
        [CanBeNull] Task<IPlayerModel> CreatePlayer(string username, string password);
        Task<bool> IsValidPassword(string pwd1, string pwd2);
    }
}
