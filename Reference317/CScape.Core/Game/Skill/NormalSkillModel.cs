using CScape.Models.Game.Entity;
using CScape.Models.Game.Skill;

namespace CScape.Core.Game.Skill
{
    public sealed class NormalSkillModel : ISkillModel
    {
        private int _cachedLevel;
        private bool _recalcLevel;

        private float _nextLevelExp;
        private float _exp;

        public SkillID Id { get; }

        public int Boost { get; set; }

        public int Level
        {
            get
            {
                RecalcLevelIfNeeded();
                return _cachedLevel + Boost;
            }
            set
            {
                _exp = GetExp(value);
                _recalcLevel = true;
            }
        }

        public float Experience
        {
            get => _exp;
            set
            {
                _recalcLevel = true;
                _exp = value;
            }
        }
       

        public NormalSkillModel(SkillID id, int boost, float exp)
        {
            Id = id;
            Boost = boost;
            Experience = exp;
            _recalcLevel = true;
        }

        private int GetExp(int level)
        {
            switch (level)
            {
                case 1: return 0;
                case 2: return 83;
                case 3: return 174;
                case 4: return 276;
                case 5: return 388;
                case 6: return 512;
                case 7: return 650;
                case 8: return 801;
                case 9: return 969;
                case 10: return 1154;
                case 11: return 1358;
                case 12: return 1584;
                case 13: return 1833;
                case 14: return 2107;
                case 15: return 2411;
                case 16: return 2746;
                case 17: return 3115;
                case 18: return 3523;
                case 19: return 3973;
                case 20: return 4470;
                case 21: return 5018;
                case 22: return 5624;
                case 23: return 6291;
                case 24: return 7028;
                case 25: return 7842;
                case 26: return 8740;
                case 27: return 9730;
                case 28: return 10824;
                case 29: return 12031;
                case 30: return 13363;
                case 31: return 14833;
                case 32: return 16456;
                case 33: return 18247;
                case 34: return 20224;
                case 35: return 22406;
                case 36: return 24815;
                case 37: return 27473;
                case 38: return 30408;
                case 39: return 33648;
                case 40: return 37224;
                case 41: return 41171;
                case 42: return 45529;
                case 43: return 50339;
                case 44: return 55649;
                case 45: return 61512;
                case 46: return 67983;
                case 47: return 75127;
                case 48: return 83014;
                case 49: return 91721;
                case 50: return 101333;
                case 51: return 111945;
                case 52: return 123660;
                case 53: return 136594;
                case 54: return 150872;
                case 55: return 166636;
                case 56: return 184040;
                case 57: return 203254;
                case 58: return 224466;
                case 59: return 247886;
                case 60: return 273742;
                case 61: return 302288;
                case 62: return 333804;
                case 63: return 368599;
                case 64: return 407015;
                case 65: return 449428;
                case 66: return 496254;
                case 67: return 547953;
                case 68: return 605032;
                case 69: return 668051;
                case 70: return 737627;
                case 71: return 814445;
                case 72: return 899257;
                case 73: return 992895;
                case 74: return 1096278;
                case 75: return 1210421;
                case 76: return 1336443;
                case 77: return 1475581;
                case 78: return 1629200;
                case 79: return 1798808;
                case 80: return 1986068;
                case 81: return 2192818;
                case 82: return 2421087;
                case 83: return 2673114;
                case 84: return 2951373;
                case 85: return 3258594;
                case 86: return 3597792;
                case 87: return 3972294;
                case 88: return 4385776;
                case 89: return 4842295;
                case 90: return 5346332;
                case 91: return 5902831;
                case 92: return 6517253;
                case 93: return 7195629;
                case 94: return 7944614;
                case 95: return 8771558;
                case 96: return 9684577;
                case 97: return 10692629;
                case 98: return 11805606;
                case 99: return 13034431;
                default: return 1;
            }
        }

        private int GetLevel()
        {
            if (Experience < 0) return 0;
            if (Experience < 83) return 1;
            if (Experience < 174) return 2;
            if (Experience < 276) return 3;
            if (Experience < 388) return 4;
            if (Experience < 512) return 5;
            if (Experience < 650) return 6;
            if (Experience < 801) return 7;
            if (Experience < 969) return 8;
            if (Experience < 1154) return 9;
            if (Experience < 1358) return 10;
            if (Experience < 1584) return 11;
            if (Experience < 1833) return 12;
            if (Experience < 2107) return 13;
            if (Experience < 2411) return 14;
            if (Experience < 2746) return 15;
            if (Experience < 3115) return 16;
            if (Experience < 3523) return 17;
            if (Experience < 3973) return 18;
            if (Experience < 4470) return 19;
            if (Experience < 5018) return 20;
            if (Experience < 5624) return 21;
            if (Experience < 6291) return 22;
            if (Experience < 7028) return 23;
            if (Experience < 7842) return 24;
            if (Experience < 8740) return 25;
            if (Experience < 9730) return 26;
            if (Experience < 10824) return 27;
            if (Experience < 12031) return 28;
            if (Experience < 13363) return 29;
            if (Experience < 14833) return 30;
            if (Experience < 16456) return 31;
            if (Experience < 18247) return 32;
            if (Experience < 20224) return 33;
            if (Experience < 22406) return 34;
            if (Experience < 24815) return 35;
            if (Experience < 27473) return 36;
            if (Experience < 30408) return 37;
            if (Experience < 33648) return 38;
            if (Experience < 37224) return 39;
            if (Experience < 41171) return 40;
            if (Experience < 45529) return 41;
            if (Experience < 50339) return 42;
            if (Experience < 55649) return 43;
            if (Experience < 61512) return 44;
            if (Experience < 67983) return 45;
            if (Experience < 75127) return 46;
            if (Experience < 83014) return 47;
            if (Experience < 91721) return 48;
            if (Experience < 101333) return 49;
            if (Experience < 111945) return 50;
            if (Experience < 123660) return 51;
            if (Experience < 136594) return 52;
            if (Experience < 150872) return 53;
            if (Experience < 166636) return 54;
            if (Experience < 184040) return 55;
            if (Experience < 203254) return 56;
            if (Experience < 224466) return 57;
            if (Experience < 247886) return 58;
            if (Experience < 273742) return 59;
            if (Experience < 302288) return 60;
            if (Experience < 333804) return 61;
            if (Experience < 368599) return 62;
            if (Experience < 407015) return 63;
            if (Experience < 449428) return 64;
            if (Experience < 496254) return 65;
            if (Experience < 547953) return 66;
            if (Experience < 605032) return 67;
            if (Experience < 668051) return 68;
            if (Experience < 737627) return 69;
            if (Experience < 814445) return 70;
            if (Experience < 899257) return 71;
            if (Experience < 992895) return 72;
            if (Experience < 1096278) return 73;
            if (Experience < 1210421) return 74;
            if (Experience < 1336443) return 75;
            if (Experience < 1475581) return 76;
            if (Experience < 1629200) return 77;
            if (Experience < 1798808) return 78;
            if (Experience < 1986068) return 79;
            if (Experience < 2192818) return 80;
            if (Experience < 2421087) return 81;
            if (Experience < 2673114) return 82;
            if (Experience < 2951373) return 83;
            if (Experience < 3258594) return 84;
            if (Experience < 3597792) return 85;
            if (Experience < 3972294) return 86;
            if (Experience < 4385776) return 87;
            if (Experience < 4842295) return 88;
            if (Experience < 5346332) return 89;
            if (Experience < 5902831) return 90;
            if (Experience < 6517253) return 91;
            if (Experience < 7195629) return 92;
            if (Experience < 7944614) return 93;
            if (Experience < 8771558) return 94;
            if (Experience < 9684577) return 95;
            if (Experience < 10692629) return 96;
            if (Experience < 11805606) return 97;
            if (Experience < 13034431) return 98;
            return 99;
        }

        private void RecalcLevelIfNeeded()
        {
            if (!_recalcLevel) return;
            _cachedLevel = GetLevel();
            _recalcLevel = false;

            _nextLevelExp = GetExp(_cachedLevel + 1);
        }

        public bool GainExperience(IEntity ent, float exp)
        {
            var nextExp = _nextLevelExp;

            Experience += exp;

            return Experience >= nextExp;
        }
    }
}
