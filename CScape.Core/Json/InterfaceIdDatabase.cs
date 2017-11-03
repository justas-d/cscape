using System.IO;
using CScape.Core.Json.Model;
using Newtonsoft.Json;

namespace CScape.Core.Json
{
    public sealed class InterfaceIdDatabase : IInterfaceIdDatabase
    {
        public int AttackLevelUpDialog { get; }
        public int DefenceLevelUpDialog { get; }
        public int StrengthLevelUpDialog { get; }
        public int HitpointsLevelUpDialog { get; }
        public int RangedLevelUpDialog { get; }
        public int PrayerLevelUpDialog { get; }
        public int MagicLevelUpDialog { get; }
        public int CookingLevelUpDialog { get; }
        public int WoodcuttingLevelUpDialog { get; }
        public int FletchingLevelUpDialog { get; }
        public int FishingLevelUpDialog { get; }
        public int FiremakingLevelUpDialog { get; }
        public int CraftingLevelUpDialog { get; }
        public int SmithingLevelUpDialog { get; }
        public int MiningLevelUpDialog { get; }
        public int HerbloreLevelUpDialog { get; }
        public int AgilityLevelUpDialog { get; }
        public int ThievingLevelUpDialog { get; }
        public int SlayerLevelUpDialog { get; }
        public int FarmingLevelUpDialog { get; }
        public int RunecraftingLevelUpDialog { get; }
        public int BackpackSidebar { get; }
        public int ControlsSidebar { get; }
        public int BackpackContainer { get; }
        public int EquipmentSidebar { get; }
        public int SkillSidebar { get; }
        public int QuestSidebar { get; }
        public int PrayerSidebar { get; }
        public int StandardSpellbookSidebar { get; }
        public int FriendsListSidebar { get; }
        public int IgnoreListSidebar { get; }
        public int LogoutSidebar { get; }
        public int OptionsLowDetailSidebar { get; }
        public int OptionsHighDetailSidebar { get; }
        public int EquipmentContainer { get; }
        public byte CombatStyleSidebarIdx { get; }
        public byte SkillSidebarIdx { get; }
        public byte QuestSidebarIdx { get; }
        public byte PrayerSidebarIdx { get; }
        public byte SpellbookSidebarIdx { get; }
        public byte FriendsSidebarIdx { get; }
        public byte IgnoresSidebarIdx { get; }
        public byte LogoutSidebarIdx { get; }
        public byte OptionsSidebarIdx { get; }
        public byte ControlsSidebarIdx { get; }
        public byte BackpackSidebarIdx { get; }
        public byte EquipmentSidebarIdx { get; }

        public InterfaceIdDatabase(int backpackSidebar, int backpackContainer,
            int equipmentSidebar, int equipmentContainer,
            byte backpackSidebarIdx, byte equipmentSidebarIdx, int controlsSidebar,
            int skillSidebar, int questSidebar,
            int standardSpellbookSidebar, int prayerSidebar,
            int friendsListSidebar, int ignoreListSidebar,
            int logoutSidebar, int optionsLowDetailSidebar,
            int optionsHighDetailSidebar, byte combatStyleSidebarIdx,
            byte skillSidebarIdx, byte questSidebarIdx, byte prayerSidebarIdx,
            byte spellbookSidebarIdx, byte friendsSidebarIdx, byte ignoresSidebarIdx,
            byte logoutSidebarIdx, byte optionsSidebarIdx,
            byte controlsSidebarIdx, int attackLevelUpDialog, int defenceLevelUpDialog,
            int strengthLevelUpDialog, int hitpointsLevelUpDialog, int rangedLevelUpDialog,
            int prayerLevelUpDialog, int magicLevelUpDialog, int cookingLevelUpDialog,
            int woodcuttingLevelUpDialog, int fletchingLevelUpDialog, int fishingLevelUpDialog,
            int firemakingLevelUpDialog, int craftingLevelUpDialog, int smithingLevelUpDialog,
            int herbloreLevelUpDialog, int agilityLevelUpDialog, int thievingLevelUpDialog,
            int miningLevelUpDialog, int slayerLevelUpDialog, int farmingLevelUpDialog,
            int runecraftingLevelUpDialog)
        {
            BackpackSidebar = backpackSidebar;
            BackpackContainer = backpackContainer;
            EquipmentSidebar = equipmentSidebar;
            EquipmentContainer = equipmentContainer;
            BackpackSidebarIdx = backpackSidebarIdx;
            EquipmentSidebarIdx = equipmentSidebarIdx;
            ControlsSidebar = controlsSidebar;
            SkillSidebar = skillSidebar;
            QuestSidebar = questSidebar;
            StandardSpellbookSidebar = standardSpellbookSidebar;
            PrayerSidebar = prayerSidebar;
            FriendsListSidebar = friendsListSidebar;
            IgnoreListSidebar = ignoreListSidebar;
            LogoutSidebar = logoutSidebar;
            OptionsLowDetailSidebar = optionsLowDetailSidebar;
            OptionsHighDetailSidebar = optionsHighDetailSidebar;
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
            AttackLevelUpDialog = attackLevelUpDialog;
            DefenceLevelUpDialog = defenceLevelUpDialog;
            StrengthLevelUpDialog = strengthLevelUpDialog;
            HitpointsLevelUpDialog = hitpointsLevelUpDialog;
            RangedLevelUpDialog = rangedLevelUpDialog;
            PrayerLevelUpDialog = prayerLevelUpDialog;
            MagicLevelUpDialog = magicLevelUpDialog;
            CookingLevelUpDialog = cookingLevelUpDialog;
            WoodcuttingLevelUpDialog = woodcuttingLevelUpDialog;
            FletchingLevelUpDialog = fletchingLevelUpDialog;
            FishingLevelUpDialog = fishingLevelUpDialog;
            FiremakingLevelUpDialog = firemakingLevelUpDialog;
            CraftingLevelUpDialog = craftingLevelUpDialog;
            SmithingLevelUpDialog = smithingLevelUpDialog;
            HerbloreLevelUpDialog = herbloreLevelUpDialog;
            AgilityLevelUpDialog = agilityLevelUpDialog;
            ThievingLevelUpDialog = thievingLevelUpDialog;
            MiningLevelUpDialog = miningLevelUpDialog;
            SlayerLevelUpDialog = slayerLevelUpDialog;
            FarmingLevelUpDialog = farmingLevelUpDialog;
            RunecraftingLevelUpDialog = runecraftingLevelUpDialog;
        }

        public static InterfaceIdDatabase FromJson(string dir)
            => JsonConvert.DeserializeObject<InterfaceIdDatabase>(File.ReadAllText(dir));
    }

}
