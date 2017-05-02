using JetBrains.Annotations;

namespace CScape.Network
{
    public interface IPlayerLogin
    {
        void Transfer([NotNull] MainLoop loop);
    }
}