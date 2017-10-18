using CScape.Core.Game.Entities.Component;
using CScape.Core.Game.Entities.Message;

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
            Requestee.Get().SystemMessage($"Talks to {TalkTarget}", SystemMessageFlags.Debug);
            TalkTarget.Get().SystemMessage($"Talks to {Requestee}", SystemMessageFlags.Debug);
        }
    }
}