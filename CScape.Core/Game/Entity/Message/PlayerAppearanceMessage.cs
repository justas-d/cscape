using System;
using CScape.Core.Game.Item;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Message
{
    public sealed class PlayerAppearanceMessage : IGameMessage
    {
        [NotNull]
        public string Username { get; }
        public PlayerAppearance Appearance { get; }

        [NotNull]
        public PlayerEquipmentContainer Equipment { get; }

        public int EventId => (int) MessageId.UpdatePlayerAppearance;

        public PlayerAppearanceMessage(string username, PlayerAppearance appearance, [NotNull] PlayerEquipmentContainer equipment)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("message", nameof(username));
            }

            Username = username;
            Appearance = appearance;
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        }
    }
}
