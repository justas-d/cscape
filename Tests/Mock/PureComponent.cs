using CScape.Models.Game.Entity;

namespace CScape.Dev.Tests.Mock
{
    public sealed class PureComponent : IEntityComponent
    {
        public IEntity Parent { get; }
        public int Priority => 0;

        public PureComponent(IEntity parent)
        {
            Parent = parent;
        }

        public void ReceiveMessage(IGameMessage msg)
        {
            
        }
    }
}