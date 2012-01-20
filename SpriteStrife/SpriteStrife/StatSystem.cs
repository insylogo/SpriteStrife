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

        public StatSystem(int[] newStats)
        {
            stats = new Stat[9];

            for (int i = 0; i < stats.Length; ++i)
            {
                stats[i] = new Stat((StatType)i, newStats[i]);
            }

        }

        public StatSystem(double strength, double toughness, int health, 
                          int intellect, int perception, int sanity,
                          int faith, int wisdom, int vitality)
        {
            stats = new Stat[9];
            stats[0] = new Stat(StatType.Strength, strength);
            stats[1] = new Stat(StatType.Toughness, toughness);
            stats[2] = new Stat(StatType.Health, health);
            stats[3] = new Stat(StatType.Intellect, intellect);
            stats[4] = new Stat(StatType.Perception, perception);
            stats[5] = new Stat(StatType.Sanity, sanity);
            stats[6] = new Stat(StatType.Faith, faith);
            stats[7] = new Stat(StatType.Wisdom, wisdom);
            stats[8] = new Stat(StatType.Vitality, vitality);
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

        public double GetStat(StatType type)
        {
            return stats[(int)type].MaxValue;
        }

        public double GetBaseStat(StatType type)
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

        public double GetModifier(StatType type)
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
