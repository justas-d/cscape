using System;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class EntityHandle : IEquatable<EntityHandle>
    {
        public EntitySystem System { get; }
        public int Generation { get; }
        public int Id { get; }

        private readonly int _baked; 

        public EntityHandle([NotNull] EntitySystem system, int generation, int id)
        {
            System = system ?? throw new ArgumentNullException(nameof(system));
            Generation = generation;
            Id = id;

            _baked = (Id << EntitySystem.GenerationBits) | Generation;
        }

        public bool IsDead() => System.IsDead(this);
        public Entity Get() => System.Get(this);
        
        public bool Equals(EntityHandle other)
        {
            if (Object.ReferenceEquals(null, other)) return false;
            if (Object.ReferenceEquals(this, other)) return true;

            return _baked == other._baked;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(null, obj)) return false;
            if (Object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            return Equals((EntityHandle) obj);
        }

        public override int GetHashCode() => _baked;
        public override string ToString() => $"Entity handle: Id: {Id} Generation: {Generation} Baked: {_baked}";
    }
}