using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Interface
{
    /// <summary>
    /// Updates run during the sync pass
    /// </summary>
    public interface IEntityNetFragment : IEntityFragment
    {
        void Update([NotNull]IMainLoop loop);
    }
}