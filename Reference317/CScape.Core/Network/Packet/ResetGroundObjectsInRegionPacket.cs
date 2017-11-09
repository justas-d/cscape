
using CScape.Models.Data;

namespace CScape.Core.Network.Packet
{
    /// <summary>
    /// Resets ground objects and Class30_Sub1.anInt1294 
    /// in the 8x8 whose top-left corner is given by the origin parameter.
    /// </summary>
    public class ResetGroundObjectsInRegionPacket : IPacket
    {
        private readonly byte _x;
        private readonly byte _y;

        public bool IsValid { get; } = true;

        public const int MaxValue = 104;
        public const int Id = 64;

        public ResetGroundObjectsInRegionPacket((int x, int y) region)
        {
            bool IsNotInRange(int val) => 0 > val || val >= MaxValue;

            if (IsNotInRange(region.x)) IsValid = false;
            if (IsNotInRange(region.y)) IsValid = false;

            if (IsValid)
            {
                _x = (byte)region.x;
                _y = (byte)region.y;
            }
        }

        public void Send(OutBlob stream)
        {
            if (!IsValid) return;

            stream.BeginPacket(Id);

            stream.Write(_x);
            stream.Write(_y);

            stream.EndPacket();
        }
    }
}