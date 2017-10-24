using CScape.Core.Game.Entity.Component;
using CScape.Models.Data;
using CScape.Models.Game;
using CScape.Models.Game.Entity.Component;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Segment
{
    public sealed class LocalPlayerInitSegment : IUpdateSegment
    {
        private readonly bool _needsUpdate;   
        private IPosition _local;

        public LocalPlayerInitSegment([NotNull]IPlayerComponent player, bool needsUpdate)
        {
            var local = player.Parent.Components.AssertGet<ClientPositionComponent>();
            _needsUpdate = needsUpdate;
            _local = local.Local;
        }

        public LocalPlayerInitSegment(IPosition pos, bool needsUpdate)
        {
            _local = pos;
            _needsUpdate = needsUpdate;
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(1, 1); // continue reading?
            stream.WriteBits(2, 3); // type

            stream.WriteBits(2, _local.Z); // plane
            stream.WriteBits(1, 1); // todo :  setpos flag
            stream.WriteBits(1, _needsUpdate ? 1 : 0); // add to needs updating list

            stream.WriteBits(7, _local.Y); // local y
            stream.WriteBits(7, _local.Z); // local x
        }
    }
}