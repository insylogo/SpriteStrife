using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteStrife
{

    enum StatType { 
        Strength = 0, Toughness, Health,
        Intellect, Perception, Sanity,
        Faith, Wisdom, Vitality
    }

    [Serializable]
    class Stat
    {

        public Stat(StatType type, int startingValue)
        {
            this.Type = type;
            modifier = 0;
            depletion = 0;
            basevalue = startingValue;
        }

        private int modifier;
        private int basevalue;
        private int depletion;
        
        public StatType Type { get; set; }

        public int MaxValue { 
            get {
                return basevalue + modifier;
            }
        }
        public int Value
        {
            get
            {
                return basevalue + modifier - depletion;
            }
        }
        public int Modifier {
            get
            {
                return modifier;
            }
            set
            {
                modifier = value;
            }
        }
        public int BaseValue { 
            get
            {
                return basevalue;   
            }
            set
            {
                basevalue = value;
            }
        }

        public void Drain(int drainAmt)
        {
            depletion += drainAmt;
            if (depletion > MaxValue) depletion = MaxValue;
        }

        public void Restore(int restoAmt)
        {
            depletion -= restoAmt;
            if (depletion < 0) depletion = 0;
        }
    }
}
