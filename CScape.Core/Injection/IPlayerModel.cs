using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IPlayerModel
    {
        [NotNull]  IPlayerAppearance Appearance { get; set; }
        [NotNull] IItemProvider BackpackItems { get; set; }

        [NotNull] string Id { get; set; }
        bool IsMember { get; set; }

        [NotNull] string PasswordHash { get; set; }
        byte TitleIcon { get; set; }

        int X { get; set; }
        int Y { get; set; }
        byte Z { get; set; }
    }
}