namespace CScape.Core.Injection
{
    public interface IInterfaceIdDatabase
    {
        int BackpackInterface { get; }
        int BackpackInventory { get; }
        int EquipmentInterface { get; }
        int EquipmentInventory { get; }

        int BackpackSidebarIdx { get; }
        int EquipmentSidebarIdx { get; }
    }
}