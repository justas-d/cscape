using JetBrains.Annotations;

namespace CScape.Core.Game.Item
{
    /// <summary>
    /// Defines a lookup for attack styles based on the button id of the owning interface pointed to by <see cref="CombatInterfaceId"/>
    /// </summary>
    public interface IWeaponCombatType
    {
        int CombatInterfaceId { get; }
        [CanBeNull] IAttackStyle GetStyleFromButton(int buttonId);
    }
}