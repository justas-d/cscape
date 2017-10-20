using System;
using CScape.Core.Game.World;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Message
{
    public sealed class PoeSwitchMessage : IGameMessage
    {
        [CanBeNull]
        public PlaneOfExistence OldPoe { get; }

        [NotNull]
        public PlaneOfExistence NewPoe { get; }

        public PoeSwitchMessage([CanBeNull] PlaneOfExistence oldPoe, [NotNull] PlaneOfExistence newPoe)
        {
            OldPoe = oldPoe;
            NewPoe = newPoe ?? throw new ArgumentNullException(nameof(newPoe));
        }

        public int EventId => MessageId.PoeSwitch;
    }
}