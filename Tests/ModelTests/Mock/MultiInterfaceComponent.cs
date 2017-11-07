using CScape.Models.Game.Entity;

namespace CScape.Dev.Tests.ModelTests.Mock
{
    public sealed class MultiInterfaceComponent : IComponentOne, IComponentTwo, IComponentThree
    {
        public IEntity Parent { get; }
        public int Priority => 0;

        public int NumReceivedMessages { get; private set; }

        public MultiInterfaceComponent(IEntity ent)
        {
            Parent = ent;
        }

        public void ReceiveMessage(IGameMessage msg)
        {
            NumReceivedMessages++;
        }
    }
}