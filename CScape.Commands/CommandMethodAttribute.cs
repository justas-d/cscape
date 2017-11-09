using System;

namespace CScape.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandMethodAttribute : Attribute
    {
        public string Identifier { get; }

        // uses method name (all lowercase)
        public CommandMethodAttribute()
        {

        }

        public CommandMethodAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}