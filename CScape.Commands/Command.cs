using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CScape.Commands
{
    public sealed class Command : IEquatable<Command>
    {
        public Command(
            [NotNull] string identifier, 
            [CanBeNull] Action commandCallbackNoContext, 
            [CanBeNull] Action<CommandContext> commandCallaback,
            [NotNull] IEnumerable<PredicateAttribute> predicates)
        {
            if(commandCallbackNoContext == null && commandCallaback == null) throw new ArgumentException("No valid exec target");
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            CommandCallbackNoContext = commandCallbackNoContext;
            CommandCallaback = commandCallaback;
            Predicates = predicates ?? throw new ArgumentNullException(nameof(predicates));
        }

        [NotNull] public string Identifier { get; }
        /// <summary>
        /// Command callback that does not take a <see cref="CommandContext"/>
        /// </summary>
        [CanBeNull] public Action CommandCallbackNoContext { get; }
        [CanBeNull] public Action<CommandContext> CommandCallaback { get; }
        [CanBeNull] public IEnumerable<PredicateAttribute> Predicates { get; }

        public bool Equals(Command other)
        {
            if (Object.ReferenceEquals(null, other)) return false;
            if (Object.ReferenceEquals(this, other)) return true;
            return string.Equals(Identifier, other.Identifier, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(null, obj)) return false;
            if (Object.ReferenceEquals(this, obj)) return true;
            return obj is Command && Equals((Command) obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Identifier);
        }
    }
}