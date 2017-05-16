namespace CScape.Core.Injection
{
    public interface IInterfaceIdDatabase
    {
        int BackpackSidebarInterface { get; }
        int EquipmentSidebarInterface { get; }

        int SkillSidebarInterface { get; }
        int QuestSidebarInterface { get; }
        int PrayerSidebarInterface { get; }
        int StandardSpellbookSidebarInterface { get; }
        int FriendsListSidebarInterface { get; }
        int IgnoreListSidebarInterface { get; }
        int LogoutSidebarInterface { get; }
        int OptionsLowDetailSidebarInterface { get; }
        int OptionsHighDetailSidebarInterface { get; }
        int ControlsSidebarInterface { get; }

        int BackpackInventory { get; }
        int EquipmentInventory { get; }

        int CombatStyleSidebarIdx { get; }
        int SkillSidebarIdx { get; }
        int QuestSidebarIdx { get; }
        int PrayerSidebarIdx { get; }
        int SpellbookSidebarIdx { get; }
        int FriendsSidebarIdx { get; }
        int IgnoresSidebarIdx { get; }
        int LogoutSidebarIdx { get; }
        int OptionsSidebarIdx { get; }
        int ControlsSidebarIdx { get; }
        int BackpackSidebarIdx { get; }
        int EquipmentSidebarIdx { get; }
    }
}