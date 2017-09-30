using System;
using CScape.Core.Game.Entities.Prefab;

namespace CScape.Core.Game.Entities
{
    public class EntityComponentNotSatisfied : EntityComponentError
    {
        public string Msg { get; }

        public EntityComponentNotSatisfied(Type type, string msg) : base(type)
        {
            Msg = msg;
        }

        public override string ToString()
        {
            return $"Component is not satisfied. {Msg} {base.ToString()}";
        }
    }

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

    public class EntityPrefabInstantiationFailure : Exception
    {
        public EntityPrefab WhenInstantiating { get; }
        public ComponentPrefab FailedComponentPrefab { get; }
        public string Reason { get; }

        public EntityPrefabInstantiationFailure(EntityPrefab whenInstantiating,  
            ComponentPrefab failedComponentPrefab, string reason)
        {
            WhenInstantiating = whenInstantiating;
            FailedComponentPrefab = failedComponentPrefab;
            Reason = reason;
        }

        public override string ToString()
        {
            return $"Failed instantiating entity prefab \"{WhenInstantiating.Name}\": {Reason} {base.ToString()}";
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