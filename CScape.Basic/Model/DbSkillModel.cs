using System.Linq;
using CScape.Core.Game.Entity;

namespace CScape.Basic.Model
{
    public class DbSkillModel : PlayerModelLeaf, ISkillProvider
    {
        public int[] Boost { get; private set; }
        public int[] Experience { get; private set; }

        public string DbBoost
        {
            get => string.Join<int>(";", Boost);
            set => Boost = value.Split(';').Select(int.Parse).ToArray();
        }

        public string DbExperience
        {
            get => string.Join<int>(";", Experience);
            set => Experience = value.Split(';').Select(int.Parse).ToArray();
        }

        public DbSkillModel()
        {
            
        }

        public DbSkillModel(int numSkills)
        {
            Boost = new int[numSkills];
            Experience = new int[numSkills];
        }
    }
}