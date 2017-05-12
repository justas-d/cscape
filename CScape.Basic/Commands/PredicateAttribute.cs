using System;
using CScape.Core.Game.Entity;

namespace CScape.Basic.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class PredicateAttribute : Attribute
    {
        public abstract bool CanExecute(Player player, Command command);
    }
}