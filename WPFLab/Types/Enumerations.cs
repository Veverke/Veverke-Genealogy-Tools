using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenealogySoftwareV3.Types
{
    public enum DateType { Between, Range, Exact, About }
    public enum CauseOfDeath { Illness, Natural, Accident, Murder, Holocaust }
    public enum AttachedFileType { Document, Image }
    public enum DocumentType { Birth, Marriage, Death }
    public enum GEDCOMTags { NAME, GIVN, SURN, SEX, BIRT, DATE, PLAC, DEAT, CAUS, FAMS, FAMC, NICK, MARR, HUSB, WIFE, CHIL, Unknown}
    public enum Sex { M, F }
}
