using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteStrife
{
    class HeroClass
    {

        private StatSystem startingStats;
        private StatSystem levelupStats;
        private Power[] powers;
        private int[] levelRequirements;
        private int count;
        private int unlockedPowers;

        public String Name
        {
            get;
            set;
        }

        public Power[] Powers { 
            get {
                return powers;
            }
        }

        public Power[] AvailablePowers
        {
            
        }

        public void AddPower(Power newPower, int levelReq = 0)
        {
            if (count + 1 > powers.Length)
            {
                Power[] swap = new Power[(int)(powers.Length * 1.5)];
                for (int i = 0; i < count; ++i) {
                    swap[i] = powers[i];
                }
                powers = swap;
            }
            powers[count++] = newPower;

        }


        public HeroClass(string name, StatSystem initStats, StatSystem lvlStats)
        {
            levelRequirements = new int[10];
            count = 0;
            powers = new Power[10];
            Name = name;
            startingStats = new StatSystem();
            levelupStats = new StatSystem();

        }


    }
}
