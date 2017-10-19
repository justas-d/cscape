using System.Collections.Generic;
using CScape.Core.Game.Entities.Message;
using CScape.Core.Game.Entities.Skill;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities.Component
{
    public sealed class SkillComponent : EntityComponent, ISkillComponent
    {
        public Dictionary<SkillID, ISkillModel> All { get; }
            = new Dictionary<SkillID, ISkillModel>();

        public override int Priority { get; }

        public SkillComponent([NotNull] Entity parent) : base(parent)
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
                Parent.SystemMessage($"Tried to gain experience in skill {skill} but no model for it was found.", SystemMessageFlags.Debug | SystemMessageFlags.Skill);
                return;
            }

            Parent.SendMessage(
                new GameMessage(
                    this, GameMessage.Type.GainExperience, new ExperienceGainMetadata(exp, model)));

            if (model.GainExperience(exp))
            {
                Parent.SendMessage(
                    new GameMessage(
                        this, GameMessage.Type.LevelUp, model));
            }
        }
        
        public override void ReceiveMessage(GameMessage msg)
        {
            
        }
    }
}