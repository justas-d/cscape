using CScape.Core.Data;

namespace CScape.Core.Network
{
    public class SetDialogInterfacePacket : IPacket
    {
        private readonly short _id;

        public const int Id = 218;

        public static SetDialogInterfacePacket Close { get; } = new SetDialogInterfacePacket(-1);

        public SetDialogInterfacePacket(short id)
        {
            _id = id;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);
            stream.Write16(_id);
            stream.EndPacket();
        }
    }

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