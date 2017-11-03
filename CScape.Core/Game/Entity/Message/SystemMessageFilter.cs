using System;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class SystemMessageFilter
    {
        private ulong _ignored = 0;

        public void Filter(ulong msg)
        {
            _ignored |= msg;
        }

        public void UnFilter(ulong msg)
        {
            _ignored &= ~msg;
        }

        public bool IsFiltered([NotNull] SystemMessage msg)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            return (msg.Flags & _ignored) != 0;
        }
    }
}