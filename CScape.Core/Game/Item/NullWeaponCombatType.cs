namespace CScape.Core.Game.Item
{
    public sealed class NullWeaponCombatType : IWeaponCombatType
    {
        public static NullWeaponCombatType Instance { get; } = new NullWeaponCombatType();

        private NullWeaponCombatType()
        {
            
        }

        public int CombatInterfaceId => -1;

        public IAttackStyle GetStyleFromButton(int buttonId) => NullAttackStyle.Instance;
    }
}
