using CScape.Core.Game.Entities.Interface;
using CScape.Core.Network;

namespace CScape.Core.Game.Interface.Showable
{
    public class LevelUpDialogInterface : SingleUserShowableInterface
    {
        private readonly string _skill;
        private readonly int _newLevel;

        public LevelUpDialogInterface(
            int id, string skill, int newLevel) 
            : base(id, null)
        {
            _skill = skill;
            _newLevel = newLevel;

            // todo: LevelUpDialogInterface button handler
        }

        protected override bool InternalRegister(IInterfaceManagerApiBackend api)
        {
            if (!api.Frontend.CanShow(InterfaceType.Chat))
                return false;

            api.Chat = this;
            return true;
        }

        protected override void InternalUnregister()
        {
            if (Api == null) return;
            Api.Chat = null;
        }

        protected override bool CanCloseRightNow() => true;

        protected override void InternalClose()
        {
            PushUpdate(SetDialogInterfacePacket.Close);
        }

        public override void Show()
        {
            // todo : proper articles for level up dialog text1
            PushUpdate(new SetInterfaceTextPacket(Id + 1, $"Congratulations, you just advanced a {_skill} level."));
            PushUpdate(new SetInterfaceTextPacket(Id + 2, $"Your {_skill} level is now {_newLevel}"));
            PushUpdate(new SetDialogInterfacePacket((short)Id));
        }
    }
}
