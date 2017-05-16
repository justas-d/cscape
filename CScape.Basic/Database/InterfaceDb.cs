using CScape.Core.Injection;

namespace CScape.Basic.Database
{
    public sealed class InterfaceDb : IInterfaceIdDatabase
    {
        public int BackpackInterface { get; }
        public int BackpackInventory { get; }
        public int EquipmentInterface { get; }
        public int EquipmentInventory { get; }
        public int BackpackSidebarIdx { get; }
        public int EquipmentSidebarIdx { get; }

        public InterfaceDb(int backpackInterface, int backpackInventory, int equipmentInterface, int equipmentInventory, int backpackSidebarIdx, int equipmentSidebarIdx)
        {
            BackpackInterface = backpackInterface;
            BackpackInventory = backpackInventory;
            EquipmentInterface = equipmentInterface;
            EquipmentInventory = equipmentInventory;
            BackpackSidebarIdx = backpackSidebarIdx;
            EquipmentSidebarIdx = equipmentSidebarIdx;
        }
    }
}
