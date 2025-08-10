using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HelperLibrary
{
    public class TY_GEN_CEVAP
    {
        public int KOD { get; set; }
        public string TURU { get; set; }
        public string ACIKLAMA { get; set; }
        public string DBS_HATA { get; set; }
        public int ALAN_KOD { get; set; }
        public string ALAN_AD { get; set; }
    }
}
