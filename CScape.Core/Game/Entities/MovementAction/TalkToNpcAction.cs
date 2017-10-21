using CScape.Core.Extensions;
using CScape.Core.Game.Entities.Message;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;

namespace CScape.Core.Game.Entities.MovementAction
{
    public class TalkToNpcAction : IMovementDoneAction
    {
        public IEntityHandle Requestee { get; }
        public IEntityHandle TalkTarget { get; }

        public TalkToNpcAction(IEntityHandle requestee, IEntityHandle target)
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