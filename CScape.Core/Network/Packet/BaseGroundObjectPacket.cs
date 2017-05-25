using CScape.Core.Data;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// The base packet for any ground object synchronization.
    /// </summary>
    public abstract class BaseGroundObjectPacket : IPacket
    {
        protected byte PackedPos { get; }

        public bool IsInvalid { get; protected set;  }
        public const int MaxOffset = 7;

        protected BaseGroundObjectPacket(int offX, int offY)
        {
            if (offX > MaxOffset || offY > MaxOffset)
                IsInvalid = true;
            else
                PackedPos = (byte) (offX << 4 | offY);
        }

        public void Send(OutBlob stream)
        {
            if (IsInvalid)
                return;

            InternalSend(stream);
        }

        protected abstract void InternalSend(OutBlob stream);
    }
}