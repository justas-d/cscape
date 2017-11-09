using System;

namespace CScape.Core.Game.Entity.Component
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class RequiresComponent : Attribute
    {
        public Type ComponentType { get; }

        public RequiresComponent(Type fragType)
        {
            ComponentType = fragType;
        }
    }
}