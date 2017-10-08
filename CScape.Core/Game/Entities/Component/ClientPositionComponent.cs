using CScape.Core.Game.Entities;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Injection;

namespace CScape.Core.Game.Entity
{
    public sealed class ClientPositionComponent : IEntityComponent
    {
        private (int x, int y) _local;
        private (int x, int y) _clientRegion;
        public const int MinRegionBorder = 16;
        public const int MaxRegionBorder = 88;

        public (int x, int y) Base { get; private set; }

        public (int x, int y) ClientRegion => _clientRegion;
        public (int x, int y) Local => _local;

        public Entities.Entity Parent { get; }
        public int Priority { get; }

        public ClientPositionComponent(Entities.Entity parent)
        {
            Parent = parent;
        }

        public void Update(IMainLoop loop)
        {
        }

        public void ReceiveMessage(EntityMessage msg)
        {
            switch (msg.Event)
            {
                case EntityMessage.EventType.Teleport:
                    UpdatePosition();
                    break;

                case EntityMessage.EventType.Move:
                    UpdateOnMove(msg.AsMove().SumMovements());
                    break;
            }
        }

        private void UpdatePosition()
        {
            var t = Parent.GetTransform();

            _clientRegion = ((t.X >> 3) - 6, (t.Y >> 3) - 6);
            _local = (t.X - (8 * ClientRegion.x), t.Y - (8 * ClientRegion.y));

            Recalc();
        }

        private void UpdateOnMove((int x, int y) delta)
        {
            _local.x += delta.x;
            _local.y += delta.y;

            Recalc();
        }

        private void Recalc()
        {
            var oldRegion = _clientRegion;

            // update locals and client region
            if (Local.x < MinRegionBorder)
            {
                _local.x += 32;
                _clientRegion.x -= 4;
            }
            else if (Local.x >= MaxRegionBorder)
            {
                _local.x -= 32;
                _clientRegion.x += 4;
            }

            if (Local.y < MinRegionBorder)
            {
                _local.y += 32;
                _clientRegion.y -= 4;
            }
            else if (Local.y >= MaxRegionBorder)
            {
                _local.y -= 32;
                _clientRegion.y += 4;
            }

            if (!oldRegion.Equals(_clientRegion))
            {
                Parent.SendMessage(
                    new EntityMessage(
                        this, EntityMessage.EventType.ClientRegionChanged, 
                        _clientRegion));                
            }

            Base = (_clientRegion.x * 8, _clientRegion.y * 8);

            Parent.GetTransform().SyncLocalsToGlobals(this);
        }
    }
}