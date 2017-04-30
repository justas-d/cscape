using System;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.Commands
{
    public sealed class CommandContext
    {
        [NotNull] public Player Callee { get; }
        [NotNull] public Command CommandModel { get; }
        [CanBeNull] public string Data { get; }
        [NotNull] public string RawData { get; }

        public CommandContext([NotNull] Player callee, [NotNull] Command commandModel, [CanBeNull] string data, [NotNull] string rawData)
        {
            Callee = callee ?? throw new ArgumentNullException(nameof(callee));
            CommandModel = commandModel ?? throw new ArgumentNullException(nameof(commandModel));
            Data = data;
            RawData = rawData;
        }
    }
}