using System;
using CScape.Core.Game.World;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class PoeSwitchMessageData
    {
        [CanBeNull]
        public PlaneOfExistence OldPoe { get; }

        [NotNull]
        public PlaneOfExistence NewPoe { get; }

        public PoeSwitchMessageData([CanBeNull] PlaneOfExistence oldPoe, [NotNull] PlaneOfExistence newPoe)
        {
            OldPoe = oldPoe;
            NewPoe = newPoe ?? throw new ArgumentNullException(nameof(newPoe));
        }
    }
}