using JetBrains.Annotations;

namespace CScape.Core.Game.Interface.Showable
{
    public class BasicSidebarInterface : SingleUserSidebarInterface
    {
        public BasicSidebarInterface(int id, int sidebarIndex, [CanBeNull] IButtonHandler buttonHandler = null) : base(id, sidebarIndex, buttonHandler)
        {
        }
    }
}
