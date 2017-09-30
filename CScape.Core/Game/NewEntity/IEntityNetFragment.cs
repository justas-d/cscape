using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    /// <summary>
    /// Updates run during the sync pass
    /// </summary>
    public interface IEntityNetFragment : IEntityFragment
    {
        void Update([NotNull]IMainLoop loop);
    }
}