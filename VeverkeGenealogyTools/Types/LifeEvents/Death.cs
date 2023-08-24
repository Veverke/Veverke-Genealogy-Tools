using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFGedcomParser.Types;

namespace WPFLab.Types.LifeEvents
{
    public class Death : LifeEvent
    {
        public CauseOfDeath CauseofDeath { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
