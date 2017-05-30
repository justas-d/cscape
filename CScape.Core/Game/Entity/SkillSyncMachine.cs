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
        public bool NeedsUpdate { get; private set; }

        // packet queue
        private readonly SetSkillDataPacket[] _packets;

        public SkillSyncMachine(int numSkills)
        {
            _packets = new SetSkillDataPacket[numSkills];
        }

        private byte ClampLevel(int level) => (byte) Utils.Clamp(level, 0, byte.MaxValue);

        public void UpdateSkill(PlayerSkills.SkillModel skill)
        {
            _packets[skill.Index] = new SetSkillDataPacket(skill.Index, skill.Experience, ClampLevel(skill.Level));
            NeedsUpdate = true;
        }

        public void Synchronize(OutBlob stream)
        {
            NeedsUpdate = false;

            for (var i = 0; i < _packets.Length; i++)
            {
                if (_packets[i] != null)
                {
                    _packets[i].Send(stream);
                    _packets[i] = null;
                }
            }
        }

        public void OnReinitialize() { }
    }
}