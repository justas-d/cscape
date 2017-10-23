namespace CScape.Models.Game
{
    public interface IPlayerModel
    {
        [NotNull]  IPlayerAppearance Appearance { get; set; }
        [NotNull] IItemProvider BackpackItems { get; set; }
        [NotNull] IItemProvider Equipment { get; set; }
        [NotNull] ISkillProvider Skills { get; set; }

        [NotNull] string Id { get; set; }
        bool IsMember { get; set; }

        [NotNull] string PasswordHash { get; set; }
        byte TitleIcon { get; set; }

        int X { get; set; }
        int Y { get; set; }
        byte Z { get; set; }1

        byte Health { get; set; }
    }
}