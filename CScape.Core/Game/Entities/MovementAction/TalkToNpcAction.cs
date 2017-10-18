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

        public void Execute()
        {
            // make sure our ents are alive
            if (Requestee.IsDead()) return;
            if (TalkTarget.IsDead()) return;

            var reqEnt = Requestee.Get();
            var talkEnt = TalkTarget.Get();

            if (!reqEnt.CanSee(talkEnt))
                return;

            // todo : talk-to logic
            Requestee.Get().SystemMessage($"Talks to {TalkTarget}");
            TalkTarget.Get().SystemMessage($"Talks to {Requestee}");
        }
    }
}