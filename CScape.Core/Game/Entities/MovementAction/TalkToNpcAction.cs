using CScape.Core.Game.Entities.Component;

namespace CScape.Core.Game.Entities.MovementAction
{
    public class TalkToNpcAction : IMovementDoneAction
    {
        public EntityHandle Requestee { get; }
        public EntityHandle TalkTarget { get; }

        public TalkToNpcAction(EntityHandle requestee, EntityHandle target)
        {
            Requestee = requestee;
            TalkTarget = target;
        }

        private bool CanSee()
        {
            var reqEnt = Requestee.Get();
            var talkEnt = TalkTarget.Get();

            // can reqEnt see talkEnt
            var vision = reqEnt.Components.Get<VisionComponent>();
            if (vision != null)
                return vision.CanSee(talkEnt);
            
            // can talkEnt see reqEnt
            vision = talkEnt.Components.Get<VisionComponent>();
            if (vision != null)
                return vision.CanSee(reqEnt);

            return false;
        }

        public void Execute()
        {
            // make sure our ents are alive
            if (Requestee.IsDead()) return;
            if (TalkTarget.IsDead()) return;

            if (!CanSee())
                return;

            // todo : talk-to logic
            Requestee.Get().SystemMessage($"Talks to {TalkTarget}");
            TalkTarget.Get().SystemMessage($"Talks to {Requestee}");
        }
    }
}