using System;
using JetBrains.Annotations;

namespace CScape.Core.Game.NewEntity
{
    public sealed class EntityHandle : IEquatable<EntityHandle>
    {
        public EntityFactory Factory { get; }
        public int Generation { get; }
        public int Id { get; }

        private readonly int _baked; 

        public EntityHandle([NotNull] EntityFactory factory, int generation, int id)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Generation = generation;
            Id = id;

            _baked = (Id << EntityFactory.GenerationBits) | Generation;
        }

        bool IsDead() => Factory.IsDead(this);
        Entity Get() => Factory.Get(this);
        
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