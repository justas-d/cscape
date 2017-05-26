namespace CScape.Core.Injection
{
    public interface IInterfaceIdDatabase
    {
        int AttackLevelUpDialog { get; }
        int DefenceLevelUpDialog { get; }
        int StrengthLevelUpDialog { get; }
        int HitpointsLevelUpDialog { get; }
        int RangedLevelUpDialog { get; }
        int PrayerLevelUpDialog { get; }
        int MagicLevelUpDialog { get; }
        int CookingLevelUpDialog { get; }
        int WoodcuttingLevelUpDialog { get; }
        int FletchingLevelUpDialog { get; }
        int FishingLevelUpDialog { get; }
        int FiremakingLevelUpDialog { get; }
        int CraftingLevelUpDialog { get; }
        int SmithingLevelUpDialog { get; }
        int MiningLevelUpDialog { get; }
        int HerbloreLevelUpDialog { get; }
        int AgilityLevelUpDialog { get; }
        int ThievingLevelUpDialog { get; }
        int SlayerLevelUpDialog { get; }
        int FarmingLevelUpDialog { get; }
        int RunecraftingLevelUpDialog { get; }

        int BackpackSidebar { get; }
        int EquipmentSidebar { get; }

        int SkillSidebar { get; }
        int QuestSidebar { get; }
        int PrayerSidebar { get; }
        int StandardSpellbookSidebar { get; }
        int FriendsListSidebar { get; }
        int IgnoreListSidebar { get; }
        int LogoutSidebar { get; }
        int OptionsLowDetailSidebar { get; }
        int OptionsHighDetailSidebar { get; }
        int ControlsSidebar { get; }

        int BackpackContainer { get; }
        int EquipmentContainer { get; }

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