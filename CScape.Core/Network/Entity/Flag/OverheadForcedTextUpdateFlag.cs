using System;
using CScape.Core.Data;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class OverheadForcedTextUpdateFlag : IUpdateFlag
    {
        [NotNull]
        public string Message { get; }

        public OverheadForcedTextUpdateFlag([NotNull] string message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public FlagType Type => FlagType.OverheadText;

        public void Write(OutBlob stream)
        {
            stream.WriteString(Message);
        }
    }
}