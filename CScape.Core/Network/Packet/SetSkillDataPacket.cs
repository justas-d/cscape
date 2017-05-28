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
        public byte SkillIdx { get; set; }
        public int Exp { get; set; }
        public byte Level { get; set; }

        public const int Id = 134;

        public SetSkillDataPacket(byte skillIdx, int exp, byte level)
        {
            SkillIdx = skillIdx;
            Exp = exp;
            Level = level;
        }

        public void Send(OutBlob stream)
        {
            stream.BeginPacket(Id);

            stream.Write(SkillIdx);
            stream.Write32(Exp);
            stream.Write(Level);

            stream.EndPacket();
        }
    }
}