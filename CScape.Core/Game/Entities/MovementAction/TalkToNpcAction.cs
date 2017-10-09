using CScape.Core.Game.Entity;

namespace CScape.Core.Game.Entities.MovementAction
{
    public class TalkToNpcAction : IMovementDoneAction
    {
        private readonly Player _player;
        private readonly Npc _npc;

        public TalkToNpcAction(Player player, Npc npc)
        {
            _player = player;
            _npc = npc;
        }

        public void Execute()
        {
            if (!_player.CanSee(_npc))
                return;

            // todo : talk-to logic
            _npc.Say("Hello world!");
        }
    }
}