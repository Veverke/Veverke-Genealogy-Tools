using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace WPFGedcomParser.Types
{
    public interface IGEDCOMParser
    {
        DataTable ReadIntoDataTable(string filePath);
        void ParseData(DataTable data);
        //Dictionary<int, Individual> GetIndividuals(DataTable data);
        //Dictionary<int, Marriage> GetFamilies(DataTable data);
    }
}
