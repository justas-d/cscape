using CScape.Core.Data;
using CScape.Core.Game.Entities.Component;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Segment
{
    public sealed class LocalPlayerInitSegment : IUpdateSegment
    {
        private readonly int _zplane;
        private readonly bool _needsUpdate;
        private readonly int _localx;
        private readonly int _localy;

        public LocalPlayerInitSegment([NotNull]PlayerComponent player, bool needsUpdate)
        {
            var local = player.Parent.Components.AssertGet<ClientPositionComponent>();
            _zplane = player.Parent.GetTransform().Z;
            _needsUpdate = needsUpdate;
            _localx = local.Local.x;
            _localy = local.Local.y;
        }

        public LocalPlayerInitSegment(int zplane, bool needsUpdate, int localx, int localy)
        {
            _zplane = zplane;
            _needsUpdate = needsUpdate;
            _localx = localx;
            _localy = localy;
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // continue reading?
            stream.WriteBits(2, 3); // type

            stream.WriteBits(2, _zplane); // plane
            stream.WriteBits(1, 1); // todo :  setpos flag
            stream.WriteBits(1, _needsUpdate ? 1 : 0); // add to needs updating list

            stream.WriteBits(7, _localy); // local y
            stream.WriteBits(7, _localx); // local x
        }
    }
}