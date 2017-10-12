namespace CScape.Core.Game.Entities.Component
{
    /// <summary>
    /// Logs system messages to ILogger
    /// </summary>
    public sealed class MessageLogComponent : EntityComponent
    {
        public override int Priority { get; }


        public MessageLogComponent(Entity parent)
            :base(parent)
        {
            
        }

        public override void ReceiveMessage(GameMessage msg)
        {
            if (msg.Event == GameMessage.Type.NewSystemMessage)
            {
                var strMSg = msg.AsNewSystemMessage();
                Log.Normal(this, $"({Parent}): {strMSg}");
            }
        }
    }
}