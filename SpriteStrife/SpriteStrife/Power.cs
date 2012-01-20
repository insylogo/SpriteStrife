using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriteStrife
{
    [Serializable]
    public class Power
    {
        public bool IsPassive { get; set; }
        public TargetPattern Pattern { get; set; }
        public double range;
        public StatSystem cost;
        public StatSystem effect;
        public double cooldown;
        public double duration;
        public string Name
        {
            get;
            set;
        }

        public Power(   string name,
                        TargetPattern aoePattern,
                        double cd = 0.0, 
                        bool passive = true,
                        StatSystem statEffect = null,  
                        StatSystem statCost = null, 
                        double dur = 0.0, 
                        double firingRange = 0.0)
        {
            Name = name;
            Pattern = aoePattern;
            duration = dur;
            cooldown = cd;
            effect = statEffect;
            cost = statCost;
            duration = dur;
            range = firingRange;
        }
    }
}
