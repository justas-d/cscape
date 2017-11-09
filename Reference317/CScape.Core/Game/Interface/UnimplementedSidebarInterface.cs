using System;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Interface;

namespace CScape.Core.Game.Interface
{
    public sealed class UnimplementedSidebarInterface : IGameInterface
    {
        public int Id { get; }
        public byte SidebarIndex { get; }

        public UnimplementedSidebarInterface(int id, byte sidebarIndex)
        {
            Id = id;
            SidebarIndex = sidebarIndex;
        }

        
        public void ShowForEntity(IEntity entity)
        {
            entity.SendMessage(InterfaceMessage.Show(
                this, new ShowSidebarInterfacePacket((short)Id, SidebarIndex)));
        }

        public void CloseForEntity(IEntity entity)
        {
            entity.SendMessage(InterfaceMessage.Close(
                this, new CloseSidebarInterface(SidebarIndex)));
        }

        public void UpdateForEntity(IEntity entity)
        {
            
        }

        public void ReceiveMessage(IEntity entity, IGameMessage msg)
        {
            if (msg.EventId != (int) MessageId.ButtonClicked) return;

            var button = msg.AsButtonClicked();
            if (button.InterfaceId != Id) return;

            entity.SystemMessage($"UnimplementedSidebarInterface: received button message, ID: {button.ButtonId}",
                CoreSystemMessageFlags.Debug | CoreSystemMessageFlags.Interface);
        }

        public bool Equals(IGameInterface other) => other?.Id == Id;
    }
}
