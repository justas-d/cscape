using CScape.Core.Game.Entities.Fragment;
using CScape.Core.Game.Entities.Fragment.Component;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Interface
{
    /// <summary>
    /// Updates run during the sync pass
    /// </summary>
    [RequiresFragment(typeof(NetworkingComponent))]
    public interface IEntityNetFragment : IEntityFragment
    {
        void Update([NotNull]IMainLoop loop, [NotNull] NetworkingComponent network);
    }
}