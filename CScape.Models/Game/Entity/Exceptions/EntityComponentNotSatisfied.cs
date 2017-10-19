using System;

namespace CScape.Models.Game.Entity.Exceptions
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
}