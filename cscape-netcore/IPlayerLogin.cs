using JetBrains.Annotations;

namespace CScape
{
    public interface IPlayerLogin
    {
        void Transfer([NotNull] MainLoop loop);
    }
}