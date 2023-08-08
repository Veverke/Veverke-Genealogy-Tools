using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenealogySoftwareV3.Types
{
    public class Death : LifeEvent
    {
        public CauseOfDeath CauseofDeath { get; private set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
