using System;

namespace CScape.Core.Game.Item
{
    public sealed class NullWeaponCombatType : IWeaponCombatType
    {
        public static NullWeaponCombatType Singleton { get; } = new NullWeaponCombatType();

        public int CombatInterfaceId => -1;

        public IAttackStyle GetStyleFromButton(int buttonId) => NullAttackStyle.Singleton;
    }
}
