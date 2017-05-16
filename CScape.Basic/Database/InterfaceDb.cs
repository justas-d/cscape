using CScape.Core.Injection;

namespace CScape.Basic.Database
{
    public sealed class InterfaceDb : IInterfaceIdDatabase
    {
        public int BackpackSidebarInterface { get; }
        public int ControlsSidebarInterface { get; }
        public int BackpackInventory { get; }
        public int EquipmentSidebarInterface { get; }
        public int SkillSidebarInterface { get; }
        public int QuestSidebarInterface { get; }
        public int PrayerSidebarInterface { get; }
        public int StandardSpellbookSidebarInterface { get; }
        public int FriendsListSidebarInterface { get; }
        public int IgnoreListSidebarInterface { get; }
        public int LogoutSidebarInterface { get; }
        public int OptionsLowDetailSidebarInterface { get; }
        public int OptionsHighDetailSidebarInterface { get; }
        public int EquipmentInventory { get; }
        public int CombatStyleSidebarIdx { get; }
        public int SkillSidebarIdx { get; }
        public int QuestSidebarIdx { get; }
        public int PrayerSidebarIdx { get; }
        public int SpellbookSidebarIdx { get; }
        public int FriendsSidebarIdx { get; }
        public int IgnoresSidebarIdx { get; }
        public int LogoutSidebarIdx { get; }
        public int OptionsSidebarIdx { get; }
        public int ControlsSidebarIdx { get; }
        public int BackpackSidebarIdx { get; }
        public int EquipmentSidebarIdx { get; }

        public InterfaceDb(int backpackSidebarInterface, int backpackInventory, int equipmentSidebarInterface, int equipmentInventory,
            int backpackSidebarIdx, int equipmentSidebarIdx, int controlsSidebarInterface, int skillSidebarInterface,
            int questSidebarInterface, int standardSpellbookSidebarInterface, int prayerSidebarInterface,
            int friendsListSidebarInterface, int ignoreListSidebarInterface, int logoutSidebarInterface,
            int optionsLowDetailSidebarInterface, int optionsHighDetailSidebarInterface, int combatStyleSidebarIdx,
            int skillSidebarIdx, int questSidebarIdx, int prayerSidebarIdx, int spellbookSidebarIdx,
            int friendsSidebarIdx, int ignoresSidebarIdx, int logoutSidebarIdx, int optionsSidebarIdx,
            int controlsSidebarIdx)
        {
            BackpackSidebarInterface = backpackSidebarInterface;
            BackpackInventory = backpackInventory;
            EquipmentSidebarInterface = equipmentSidebarInterface;
            EquipmentInventory = equipmentInventory;
            BackpackSidebarIdx = backpackSidebarIdx;
            EquipmentSidebarIdx = equipmentSidebarIdx;
            ControlsSidebarInterface = controlsSidebarInterface;
            SkillSidebarInterface = skillSidebarInterface;
            QuestSidebarInterface = questSidebarInterface;
            StandardSpellbookSidebarInterface = standardSpellbookSidebarInterface;
            PrayerSidebarInterface = prayerSidebarInterface;
            FriendsListSidebarInterface = friendsListSidebarInterface;
            IgnoreListSidebarInterface = ignoreListSidebarInterface;
            LogoutSidebarInterface = logoutSidebarInterface;
            OptionsLowDetailSidebarInterface = optionsLowDetailSidebarInterface;
            OptionsHighDetailSidebarInterface = optionsHighDetailSidebarInterface;
            CombatStyleSidebarIdx = combatStyleSidebarIdx;
            SkillSidebarIdx = skillSidebarIdx;
            QuestSidebarIdx = questSidebarIdx;
            PrayerSidebarIdx = prayerSidebarIdx;
            SpellbookSidebarIdx = spellbookSidebarIdx;
            FriendsSidebarIdx = friendsSidebarIdx;
            IgnoresSidebarIdx = ignoresSidebarIdx;
            LogoutSidebarIdx = logoutSidebarIdx;
            OptionsSidebarIdx = optionsSidebarIdx;
            ControlsSidebarIdx = controlsSidebarIdx;
        }
    }
}
