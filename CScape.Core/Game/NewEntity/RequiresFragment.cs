using System;

namespace CScape.Core.Game.NewEntity
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