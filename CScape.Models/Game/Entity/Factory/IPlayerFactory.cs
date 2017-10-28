using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity.Factory
{
    /// <summary>
    /// Defines a factory for player entities.
    /// </summary>
    public interface IPlayerFactory
    {
        IEntitySystem EntitySystem { get; }
        IReadOnlyList<IEntityHandle> All { get; }

        // TODO : define IPlayerFactory

        int NumAlivePlayers { get; }

        [CanBeNull]
        IEntityHandle Get(int id);
        
        [CanBeNull]
        IEntityHandle Get(string username);
    }
}