using System;

namespace CScape.Core.Game.NewEntity
{
    public class EntityComponentError : Exception
    {
        public Type Type { get; }

        public EntityComponentError(Type type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return $"Type: {Type} {base.ToString()}";
        }
    }

    public class EntityComponentNotFound : EntityComponentError
    {
        public EntityComponentNotFound(Type type) : base(type)
        {
        }
    }

    public class EntityComponentAlreadyExists : EntityComponentError
    {
        public EntityComponentAlreadyExists(Type type) : base(type)
        {
        }
    }
   
    public class DestroyedEntityDereference : Exception
    {
        public EntityHandle Handle { get; }

        public DestroyedEntityDereference(EntityHandle handle)
        {
            Handle = handle;
        }

        public override string ToString()
        {
            return $"Handle: {Handle} Exception: {base.ToString()}";
        }
    }
}