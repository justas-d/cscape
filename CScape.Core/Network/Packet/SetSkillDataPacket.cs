using CScape.Core.Data;

namespace CScape.Core.Network.Packet
{
    public class SetSkillDataPacket : IPacket
    {
        private readonly byte _skillIdx;
        private readonly int _exp;
        private readonly byte _level;

        public const int Id = 134;

        public SetSkillDataPacket(byte skillIdx, int exp, byte level)
        {
            _skillIdx = skillIdx;
            _exp = exp;
            _level = level;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write(_skillIdx);
            stream.Write32(_exp);
            stream.Write(_level);

            stream.EndPacket();
        }
    }
}