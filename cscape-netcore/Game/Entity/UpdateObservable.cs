using System;
using JetBrains.Annotations;

namespace CScape.Game.Entity
{
    // todo : making a wrapper class for each observable can be a bit heavy on the GC. Use bit flags to mark new-ness instead?
    public sealed class UpdateObservable
    {
        public AbstractEntity Observable { get; }
        public bool IsNew { get; set; } = true;

        public UpdateObservable([NotNull] AbstractEntity observable)
        {
            Observable = observable ?? throw new ArgumentNullException(nameof(observable));
        }
    }
}