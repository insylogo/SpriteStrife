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
            return stats[(int)type].Value;
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

        public int Strength
        {
            get
            {
                return stats[(int)StatType.Strength].Value;
            }
        }

        public int Toughness
        {
            get
            {
                return stats[(int)StatType.Toughness].Value;
            }
        }

        public int Health
        {
            get
            {
                return stats[(int)StatType.Health].Value;
            }
        }

        public int Intellect
        {
            get
            {
                return stats[(int)StatType.Intellect].Value;
            }
        }

        public int Perception
        {
            get
            {
                return stats[(int)StatType.Perception].Value;
            }
        }


        public int Sanity
        {
            get
            {
                return stats[(int)StatType.Sanity].Value;
            }
        }

        public int Faith
        {
            get
            {
                return stats[(int)StatType.Faith].Value;
            }
        }
        public int Wisdom
        {
            get
            {
                return stats[(int)StatType.Wisdom].Value;
            }
        }
        public int Vitality
        {
            get
            {
                return stats[(int)StatType.Vitality].Value;
            }
        }

    }
}
