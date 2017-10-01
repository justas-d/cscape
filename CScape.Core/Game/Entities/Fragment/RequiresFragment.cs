using System;

namespace CScape.Core.Game.Entities.Fragment
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequiresFragment : Attribute
    {
        public Type FragmentType { get; }

        public RequiresFragment(Type fragType)
        {
            FragmentType = fragType;
        }
    }
}