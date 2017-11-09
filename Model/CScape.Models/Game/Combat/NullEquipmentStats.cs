
namespace CScape.Models.Game.Combat
{
    public sealed class NullEquipmentStats : IEquipmentStats
    {
        public static NullEquipmentStats Instance { get; } = new NullEquipmentStats();

        private NullEquipmentStats()
        {
            
        }

        public int Slash { get; } = 0;
        public int Crush { get; } = 0;
        public int Stab { get; } = 0;
        public int Magic { get; } = 0;
        public int Ranged { get; } = 0;
    }
}
