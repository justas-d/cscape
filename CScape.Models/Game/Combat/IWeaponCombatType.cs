using JetBrains.Annotations;

namespace CScape.Models.Game.Combat
{
    /// <summary>
    /// Defines a lookup for attack styles based on the button id of the owning interface pointed to by <see cref="CombatInterfaceId"/>
    /// </summary>
    public interface IWeaponCombatType
    {
        /// <summary>
        /// The interface id of the combat type selection interface.
        /// </summary>
        int CombatInterfaceId { get; }

        /// <summary>
        /// Maps a button that belongs to <see cref="CombatInterfaceId"/> to an <see cref="IAttackStyle"/>
        /// </summary>
        /// <returns>an <see cref="IAttackStyle"/> for the given <see cref="buttonId"/> if one exists, null otherwise.</returns>
        [CanBeNull] IAttackStyle GetStyleFromButton(int buttonId);
    }
}