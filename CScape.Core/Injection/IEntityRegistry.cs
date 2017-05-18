using System.Collections.Generic;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IEntityRegistry<TKey, TVal> where TVal : IEntity
    {
        [NotNull] IReadOnlyDictionary<TKey, TVal> All { get; }
        [CanBeNull] TVal GetById(TKey id);

        void Register([NotNull] TVal ent);
        void Unregister([NotNull] TVal pent);
    }
}