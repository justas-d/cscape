using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Network
{
    public interface IPlayerLogin
    {
        void Transfer([NotNull] IMainLoop loop);
    }
}