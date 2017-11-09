using System;

namespace CScape.Models.Game.Entity.Exceptions
{
    public class DestroyedEntityDereference : Exception
    {
        public IEntityHandle Handle { get; }

        public DestroyedEntityDereference(IEntityHandle handle)
        {
            Handle = handle;
        }

        public override string ToString()
        {
            return $"Handle: {Handle} Exception: {base.ToString()}";
        }
    }
}