using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VeverkeGenealogyTools.Types.GEDCOM.Entities;

namespace VeverkeGenealogyTools.Types.LifeEvents.Components
{
    public class Place : PieceOfData
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string Neighborhood { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", City, State, Country);
        }
    }
}
