using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;

namespace CScape.Core.Game.Entities.Fragment.Component
{
    /// <summary>
    /// Logs system messages to ILogger
    /// </summary>
    public sealed class MessageLogComponent : IEntityComponent
    {
        public Entity Parent { get; }
        public int Priority { get; }

        private ILogger Log => Parent.Log;

        public MessageLogComponent(Entity parent)
        {
            Parent = parent;
        }

        public void ReceiveMessage(EntityMessage msg)
        {
            if (msg.Event == EntityMessage.EventType.NewSystemMessage)
            {
                var strMSg = msg.AsNewSystemMessage();
                Log.Normal(this, $"({Parent}): {strMSg}");
            }
        }

        public void Update(IMainLoop loop)
        {
            
        }
    }
}