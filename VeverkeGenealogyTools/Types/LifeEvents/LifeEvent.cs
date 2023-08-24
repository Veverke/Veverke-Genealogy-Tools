﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VeverkeGenealogyTools.Types;
using VeverkeGenealogyTools.Types.LifeEvents.Components;

namespace VeverkeGenealogyTools.Types.LifeEvents
{
    public class LifeEvent
    {
        public Date Date { get; set; }
        public Place Place { get; set; }

        public LifeEvent()
        {
            Date = new Date();
            Place = new Place();
        }

        public override string ToString()
        {
            return string.Format("Date: {0}, Place: {1}", Date, Place);
        }
    }
}
