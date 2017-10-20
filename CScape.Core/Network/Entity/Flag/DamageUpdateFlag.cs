using System;
using CScape.Core.Data;
using CScape.Core.Game.Entities.Message;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Flag
{
    public sealed class DamageUpdateFlag : IUpdateFlag
    {
        [NotNull]
        public TakeDamageMessage Hit { get; }

        public DamageUpdateFlag([NotNull] TakeDamageMessage hit)
        {
            Hit = hit ?? throw new ArgumentNullException(nameof(hit));
        }

        public FlagType Type => FlagType.Damage;

        public void Write(OutBlob stream)
        {
            stream.Write(Hit.Damage);
            stream.Write((byte)Hit.Type);
            stream.Write(Hit.CurrentHealth);
            stream.Write(Hit.MaxHealth);
        }
    }
}