using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Models.Game.Entity
{
    /// <summary>
    /// Defines a lookup of active players in the world.
    /// </summary>
    public interface IPlayerCatalogue
    {
        IEntitySystem EntitySystem { get; }
        IEnumerable<IEntityHandle> All { get; }

        int NumAlivePlayers { get; }

        [CanBeNull]
        IEntityHandle Get(int id);
        
        [CanBeNull]
        IEntityHandle Get(string username);
    }
}