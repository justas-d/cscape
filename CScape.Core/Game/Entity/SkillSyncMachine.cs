using System.Linq;
using CScape.Core.Data;
using CScape.Core.Network;
using CScape.Core.Network.Sync;

namespace CScape.Core.Game.Entity
{
    public class SkillSyncMachine : ISyncMachine
    {
        public int Order => SyncMachineConstants.Skills;
        public bool RemoveAfterInitialize => false;

        // packet queue
        private readonly SetSkillDataPacket[] _packets;
        private bool _needsUpdate;

        public SkillSyncMachine(int numSkills)
        {
            _packets = new SetSkillDataPacket[numSkills];
        }

        private byte ClampLevel(int level) => (byte) Utils.Clamp(level, 0, byte.MaxValue);

        public void UpdateSkill(PlayerSkills.SkillModel skill)
        {
            if (_packets[skill.Index] == null)
                _packets[skill.Index] = new SetSkillDataPacket(skill.Index, skill.Experience, ClampLevel(skill.Level));
            else
            {
                var p = _packets[skill.Index];
                p.Level = ClampLevel(skill.Level);
                p.Exp = skill.Experience;
            }

            _needsUpdate = true;
        }

        public void Synchronize(OutBlob stream)
        {
            if (!_needsUpdate)
                return;

            _needsUpdate = false;

            foreach (var t in Enumerable.Where<SetSkillDataPacket>(_packets, p => p != null))
                t.Send(stream);
        }

        public void OnReinitialize() { }
    }
}