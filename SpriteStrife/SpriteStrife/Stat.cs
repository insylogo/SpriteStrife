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

        public Stat(StatType type, double startingValue = 0.0, double mod = 0.0, double dep = 0.0)
        {
            this.Type = type;
            modifier = mod;
            depletion = dep;
            basevalue = startingValue;
        }

        private double modifier;
        private double basevalue;
        private double depletion;
        
        public StatType Type { get; set; }

        public double MaxValue { 
            get {
                return (basevalue + modifier);
            }
        }
        public int Value
        {
            get
            {
                return (int) (basevalue + modifier - depletion);
            }
        }
        public double Modifier {
            get
            {
                return modifier;
            }
            set
            {
                modifier = value;
            }
        }
        public double BaseValue { 
            get
            {
                return basevalue;   
            }
            set
            {
                basevalue = value;
            }
        }

        public void Drain(double drainAmt)
        {
            depletion += drainAmt;
            if (depletion > MaxValue) depletion = MaxValue;
        }

        public void Restore(double restoAmt)
        {
            depletion -= restoAmt;
            if (depletion < 0) depletion = 0;
        }
    }
}
