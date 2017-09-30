using System;

namespace CScape.Core.Game.Entities
{
    public sealed class RequiresFragment : Attribute
    {
        public Type FragmentType { get; }

        public RequiresFragment(Type fragType)
        {
            FragmentType = fragType;
        }
    }
}