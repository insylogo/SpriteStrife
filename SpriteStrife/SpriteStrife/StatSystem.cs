using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteStrife
{
    [Serializable]
    class StatSystem
    {
        Stat[] stats;

        public StatSystem()
        {
            stats = new Stat[9];

            for (int i = 0; i < 9; ++i)
            {
                stats[i] = new Stat((StatType)i, 0);
            }
        }

        public Stat[] Stats
        {
            get
            {
                return stats;
            }
        }

        public StatSystem Add(StatSystem otherSystem)
        {
            StatSystem newSystem = new StatSystem();

            for (int i = 0; i < 9; ++i)
            {
                newSystem[(StatType)i].BaseValue = stats[i].BaseValue + otherSystem.stats[i].BaseValue;
                newSystem[(StatType)i].Modifier = stats[i].Modifier + otherSystem.stats[i].Modifier;
            }

            return newSystem;
        }

        public StatSystem Subtract(StatSystem otherSystem)
        {
            StatSystem newSystem = new StatSystem();

            for (int i = 0; i < 9; ++i)
            {
                newSystem[(StatType)i].BaseValue = stats[i].BaseValue - otherSystem.stats[i].BaseValue;
                newSystem[(StatType)i].Modifier = stats[i].Modifier - otherSystem.stats[i].Modifier;
            }

            return newSystem;
        }

        public Stat this[StatType type]
        {
            get { return stats[(int)type]; }
        }

        public int GetStat(StatType type)
        {
            return stats[(int)type].MaxValue;
        }

        public int GetBaseStat(StatType type)
        {
            return stats[(int)type].BaseValue;
        }

        public void SetBaseStat(StatType type, int newValue)
        {
            stats[(int)type].BaseValue = newValue;
        }

        public void SetStatModifier(StatType type, int newValue)
        {
            stats[(int)type].Modifier = newValue;
        }

        public int GetModifier(StatType type)
        {
            return stats[(int)type].Modifier;
        }

        public void DrainStat(StatType type, int drainAmt)
        {
            stats[(int)type].Drain(drainAmt);
        }
        public void RestoreStat(StatType type, int restoAmt)
        {
            stats[(int)type].Restore(restoAmt);
        }

        public int Strength
        {
            get
            {
                return stats[(int)StatType.Strength].MaxValue;
            }
        }

        public int Toughness
        {
            get
            {
                return stats[(int)StatType.Toughness].MaxValue;
            }
        }

        public int Health
        {
            get
            {
                return stats[(int)StatType.Health].MaxValue;
            }
        }

        public int Intellect
        {
            get
            {
                return stats[(int)StatType.Intellect].MaxValue;
            }
        }

        public int Perception
        {
            get
            {
                return stats[(int)StatType.Perception].MaxValue;
            }
        }


        public int Sanity
        {
            get
            {
                return stats[(int)StatType.Sanity].MaxValue;
            }
        }

        public int Faith
        {
            get
            {
                return stats[(int)StatType.Faith].MaxValue;
            }
        }
        public int Wisdom
        {
            get
            {
                return stats[(int)StatType.Wisdom].MaxValue;
            }
        }
        public int Vitality
        {
            get
            {
                return stats[(int)StatType.Vitality].MaxValue;
            }
        }

    }
}
