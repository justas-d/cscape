using CScape.Core.Data;
using CScape.Core.Game.Entities.Component;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Segment
{
    public sealed class InitPlayerSegment : IUpdateSegment
    {
        private readonly int _pid;
        private readonly bool _needsUpdate;
        private readonly int _xdelta;
        private readonly int _ydelta;

        public InitPlayerSegment(
            [NotNull] PlayerComponent newPlayer, [NotNull] PlayerComponent localPlayer,
            bool needsUpdate)
        {
            _pid = newPlayer.PlayerId;
            _needsUpdate = needsUpdate;
            _xdelta = newPlayer.Parent.GetTransform().X - localPlayer.Parent.GetTransform().X;
            _xdelta = newPlayer.Parent.GetTransform().Y - localPlayer.Parent.GetTransform().Y;
        }

        public InitPlayerSegment(int pid, bool needsUpdate, int xdelta, int ydelta)
        {
            _pid = pid;
            _needsUpdate = needsUpdate;
            _xdelta = xdelta;
            _ydelta = ydelta;
        }

        public void Write(OutBlob stream)
        {
            stream.WriteBits(11, _pid); // id
            stream.WriteBits(1, _needsUpdate ? 1 : 0); // needs update?
            stream.WriteBits(1, 1); // todo :  setpos flag
            stream.WriteBits(5, _ydelta); // ydelta
            stream.WriteBits(5, _xdelta); // xdelta            
        }
    }
}