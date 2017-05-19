using CScape.Core.Game.Entity;
using CScape.Core.Game.Interface;
using JetBrains.Annotations;

namespace CScape.Core.Injection
{
    public interface IPlayerModel
    {
        [NotNull]  IPlayerAppearance Appearance { get; set; }
        [NotNull] IItemProvider BackpackItems { get; set; }
        [NotNull] IItemProvider Equipment { get; set; }

        [NotNull] string Id { get; set; }
        bool IsMember { get; set; }

        [NotNull] string PasswordHash { get; set; }
        byte TitleIcon { get; set; }

        int X { get; set; }
        int Y { get; set; }
        byte Z { get; set; }

        /// <summary>
        /// Sets the X Y and Z values to the X Y Z values found in the given <see cref="IPosition"/>
        /// </summary>
        void SyncPosition(IPosition pos);
    }
}