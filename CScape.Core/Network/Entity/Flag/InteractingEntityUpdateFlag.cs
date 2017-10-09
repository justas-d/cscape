using System;
using CScape.Core.Data;
using CScape.Core.Game.Entities.InteractingEntity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class InteractingEntityUpdateFlag : IUpdateFlag
    {
        [NotNull]
        public IInteractingEntity Data { get; }

        public InteractingEntityUpdateFlag([NotNull] IInteractingEntity data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public FlagType Type => FlagType.InteractingEntity;

        public void Write(OutBlob stream)
        {
            stream.Write16(Data.Id);
        }
    }
}