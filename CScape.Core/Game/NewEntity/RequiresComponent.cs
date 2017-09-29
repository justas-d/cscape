using System;

namespace CScape.Core.Game.NewEntity
{
    public sealed class RequiresComponent : Attribute
    {
        public Type ComponentType { get; }

        public RequiresComponent(Type componentType)
        {
            ComponentType = componentType;
        }
    }
}