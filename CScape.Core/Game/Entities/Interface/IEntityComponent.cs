using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Interface
{
    /// <summary>
    /// Updates run during the entity update pass
    /// </summary>
    public interface IEntityComponent : IEntityFragment
    {
        void Update([NotNull]IMainLoop loop);
    }
}