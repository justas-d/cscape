namespace CScape.Core.Game.Entity
{
    public interface ISkillProvider
    {
        int[] Boost { get; }
        int[] Experience { get; }
    }
}