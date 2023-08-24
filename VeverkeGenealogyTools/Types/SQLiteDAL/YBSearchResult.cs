using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeverkeGenealogyTools.Types.SQLiteDAL
{
    public class YBSearchResult
    {
        public int Page { get; set; }
        public int Line { get; set; }
        public int Word { get; set; }

        public YBSearchResult(int page, int line, int word)
        {
            Page = page;
            Line = line;
            Word = word;
        }
    }
}
