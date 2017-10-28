using CScape.Core.Extensions;
using CScape.Core.Game.Entity.Message;
using CScape.Models.Extensions;
using CScape.Models.Game;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Entity.Component;
using CScape.Models.Game.Message;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entity.Component
{
    public sealed class ClientPositionComponent : EntityComponent, IClientPositionComponent
    {
        private MutableIntVec3 _local = new MutableIntVec3();
        private MutableIntVec3 _clientRegion = new MutableIntVec3();
        private ImmIntVec3 _base = new ImmIntVec3();
        
        public const int MinRegionBorder = 16;
        public const int MaxRegionBorder = 88;

        public IPosition Base => _base;
        public IPosition ClientRegion => _clientRegion;
        public IPosition Local => _local;

        public override int Priority => (int) ComponentPriority.ClientPositionComponent;

        public ClientPositionComponent([NotNull] IEntity parent) : base(parent)
        {
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.Teleport:
                    UpdatePosition();
                    break;

                case (int)MessageId.Move:
                    UpdateOnMove(msg.AsMove().SumMovements());
                    break;
            }
        }

        private void UpdatePosition()
        {
            var t = Parent.GetTransform();

            _clientRegion.X = (t.X >> 3) - 6;
            _clientRegion.Y = (t.Y >> 3) - 6;
            _clientRegion.Z = t.Z;

            _local.X = (t.X - (8 * _clientRegion.X));
            _local.Y = (t.Y - (8 * _clientRegion.Y));
            _local.Z = t.Z;

            Recalc();
        }

        private void UpdateOnMove((int x, int y) delta)
        {
            _local.X += delta.x;
            _local.Y += delta.y;

            Recalc();
        }

        private void Recalc()
        {
            var oldRegion = _clientRegion;

            // update locals and client region
            if (Local.X < MinRegionBorder)
            {
                _local.X += 32;
                _clientRegion.X -= 4;
            }
            else if (Local.X >= MaxRegionBorder)
            {
                _local.X -= 32;
                _clientRegion.X += 4;
            }

            if (Local.Y < MinRegionBorder)
            {
                _local.Y += 32;
                _clientRegion.Y -= 4;
            }
            else if (Local.Y >= MaxRegionBorder)
            {
                _local.Y -= 32;
                _clientRegion.Y += 4;
            }

            if (!oldRegion.Equals(_clientRegion))
            {
                Parent.SendMessage(NotificationMessage.ClientRegionChanged);
            }

            _base = new ImmIntVec3(_clientRegion.X * 8, _clientRegion.Y * 8, Parent.GetTransform().Z);

            Parent.GetTransform().SyncLocalsToGlobals(this);
        }
    }
}