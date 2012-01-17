using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteStrife
{
    [Serializable]
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
            basevalue = startingValue;
        }
        
        private int modifier;
        private int basevalue;
        
        
        public StatType Type { get; set; }

        public int Value { 
            get {
                return basevalue + modifier;
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
    }
}

