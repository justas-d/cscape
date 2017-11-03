using System.Collections.Generic;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Skill;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class SkillComponent : EntityComponent, ISkillComponent
    {
        public Dictionary<SkillID, ISkillModel> All { get; }
            = new Dictionary<SkillID, ISkillModel>();

        public override int Priority => (int) ComponentPriority.Invariant;

        public SkillComponent([NotNull] IEntity parent) : base(parent)
        {
            
        }

        [CanBeNull]
        public ISkillModel GetModel(SkillID skill)
        {
            if (All.ContainsKey(skill))
                return All[skill];
            return null;
        }

        public void GainExperience(SkillID skill, float exp)
        {
            var model = GetModel(skill);

            if (model == null)
            {
                Parent.SystemMessage($"Tried to gain experience in skill {skill} but no model for it was found.", CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Skill);
                return;
            }

            Parent.SendMessage(new ExperienceGainMessage(exp, model));

            if (model.GainExperience(Parent, exp))
            {
                Parent.SendMessage(new LevelUpMessage(model));
            }
        }
        
        public override void ReceiveMessage(IGameMessage msg)
        {
            
        }
    }
}