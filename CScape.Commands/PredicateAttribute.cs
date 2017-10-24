using System;
using CScape.Models.Game.Entity;

namespace CScape.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class PredicateAttribute : Attribute
    {
        public abstract bool CanExecute(IEntity player, Command command);
    }
}