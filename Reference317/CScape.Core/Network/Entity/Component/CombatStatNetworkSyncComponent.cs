using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(CombatStatComponent))]
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class CombatStatNetworkSyncComponent : EntityComponent
    {
        public override int Priority => (int)ComponentPriority.InvariantSync;

        private bool _isDirty = false;

        public CombatStatNetworkSyncComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        private void Sync()
        {
            // don't update if we didn't receive an EquipmentChange event
            if (!_isDirty)
                return;

            _isDirty = false;

            string Format(int num) => num >= 0 ? $"+{num}" : num.ToString();
            var net = Parent.AssertGetNetwork();
            var stats = Parent.AssertGetCombatStats();

            net.SendPacket(new SetInterfaceTextPacket(1675, $"Stab: {Format(stats.Attack.Stab)}"));
            net.SendPacket(new SetInterfaceTextPacket(1676, $"Slash: {Format(stats.Attack.Slash)}"));
            net.SendPacket(new SetInterfaceTextPacket(1677, $"Crush: {Format(stats.Attack.Crush)}"));
            net.SendPacket(new SetInterfaceTextPacket(1678, $"Magic: {Format(stats.Attack.Magic)}"));
            net.SendPacket(new SetInterfaceTextPacket(1679, $"Range: {Format(stats.Attack.Ranged)}"));

            net.SendPacket(new SetInterfaceTextPacket(1680, $"Stab: {Format(stats.Defense.Stab)}"));
            net.SendPacket(new SetInterfaceTextPacket(1681, $"Slash: {Format(stats.Defense.Slash)}"));
            net.SendPacket(new SetInterfaceTextPacket(1682, $"Crush: {Format(stats.Defense.Crush)}"));
            net.SendPacket(new SetInterfaceTextPacket(1683, $"Magic: {Format(stats.Defense.Magic)}"));
            net.SendPacket(new SetInterfaceTextPacket(1684, $"Range: {Format(stats.Defense.Ranged)}"));

            net.SendPacket(new SetInterfaceTextPacket(1685, $"Strength: {Format(stats.StrengthBonus)}     Range: {Format(stats.RangedBonus)}"));
            net.SendPacket(new SetInterfaceTextPacket(1686, $"Magic: {Format(stats.MagicBonus)}"));
            net.SendPacket(new SetInterfaceTextPacket(1687, $"Prayer: {Format(stats.PrayerBonus)}"));
        }
        
        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.EquipmentChange:
                    _isDirty = true;
                    break;
                case (int)MessageId.NetworkPrepare:
                    Sync();
                    break;
            }
        }
    }
}
