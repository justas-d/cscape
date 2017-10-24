using System;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game
{
    public sealed class PlayerModel
    {
        [NotNull]
        public string Username { get; }
        public bool IsMember { get; }
        public PlayerComponent.Title Title { get; }
        public PlayerAppearance Apperance { get; }

        public PlayerModel(
            [NotNull] string username, 
            bool isMember, 
            PlayerComponent.Title title,
            PlayerAppearance apperance)
        {

            Username = username ?? throw new ArgumentNullException(nameof(username));
            IsMember = isMember;
            Title = title;
            Apperance = apperance;
        }
    }
}
