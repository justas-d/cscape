using System;

namespace CScape.Models.Game.Entity.Exceptions
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
}