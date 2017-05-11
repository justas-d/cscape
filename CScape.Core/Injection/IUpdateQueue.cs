using System.Collections.Generic;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IUpdateQueue<T> : IEnumerable<T> where T : IEntity
    {
        int Count { get; }

        void Enqueue([NotNull] T ent);
        [NotNull] T Dequeue();
    }
}