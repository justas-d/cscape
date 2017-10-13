using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Component;
using CScape.Core.Network.Packet;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(CombatStatComponent))]
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class CombatStatNetworkSyncComponent : EntityComponent
    {
        public override int Priority { get; }

        private NetworkingComponent Network => Parent.Components.AssertGet<NetworkingComponent>();
        private CombatStatComponent Stats => Parent.Components.AssertGet<CombatStatComponent>();

        private bool _isDirty = false;

        public CombatStatNetworkSyncComponent([NotNull] Game.Entities.Entity parent) : base(parent)
        {
        }

        private void Sync()
        {
            // don't update if we didn't receive an EquipmentChange event
            if (!_isDirty)
                return;

            _isDirty = false;

            string Format(int num) => num >= 0 ? $"+{num}" : num.ToString();
            var net = Network;

            net.SendPacket(new SetInterfaceTextPacket(1675, $"Stab: {Format(Stats.Attack.Stab)}"));
            net.SendPacket(new SetInterfaceTextPacket(1676, $"Slash: {Format(Stats.Attack.Slash)}"));
            net.SendPacket(new SetInterfaceTextPacket(1677, $"Crush: {Format(Stats.Attack.Crush)}"));
            net.SendPacket(new SetInterfaceTextPacket(1678, $"Magic: {Format(Stats.Attack.Magic)}"));
            net.SendPacket(new SetInterfaceTextPacket(1679, $"Range: {Format(Stats.Attack.Ranged)}"));

            net.SendPacket(new SetInterfaceTextPacket(1680, $"Stab: {Format(Stats.Defense.Stab)}"));
            net.SendPacket(new SetInterfaceTextPacket(1681, $"Slash: {Format(Stats.Defense.Slash)}"));
            net.SendPacket(new SetInterfaceTextPacket(1682, $"Crush: {Format(Stats.Defense.Crush)}"));
            net.SendPacket(new SetInterfaceTextPacket(1683, $"Magic: {Format(Stats.Defense.Magic)}"));
            net.SendPacket(new SetInterfaceTextPacket(1684, $"Range: {Format(Stats.Defense.Ranged)}"));

            net.SendPacket(new SetInterfaceTextPacket(1685, $"Strength: {Format(Stats.StrengthBonus)}     Range: {Format(Stats.RangedBonus)}"));
            net.SendPacket(new SetInterfaceTextPacket(1686, $"Magic: {Format(Stats.MagicBonus)}"));
            net.SendPacket(new SetInterfaceTextPacket(1687, $"Prayer: {Format(Stats.PrayerBonus)}"));
        }
        
        public override void ReceiveMessage(GameMessage msg)
        {
            switch (msg.Event)
            {
                case GameMessage.Type.EquipmentChange:
                    _isDirty = true;
                    break;
                case GameMessage.Type.NetworkUpdate:
                    Sync();
                    break;
            }
        }
    }
}
