using System;
using CScape.Core.Game.Entities.Fragment.Component;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;

namespace CScape.Core.Game.Entities.Fragment.Network
{
    /// <summary>
    /// Responsible for syncing every visible player entity to the network.
    /// </summary>
    public sealed class PlayerEntitySyncNetFragment : IEntityNetFragment
    {
        public Entity Parent { get; }
        public int Priority { get; } = NetFragConstants.PriorityPlayerUpdate;

        public PlayerEntitySyncNetFragment(Entity parent)
        {
            Parent = parent;
        }

        
        public void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.TookDamage:
                {
                    // TODO : player flags
                    // TODO : sync damage
                    throw new NotImplementedException();
                    break;
                }
            }
        }

        public void Update(IMainLoop loop, NetworkingComponent network)
        {

        }
    }
}