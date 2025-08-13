using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public partial class MesajSablon
    {
        partial void OnCreated();

        public MesajSablon() : base()
        {
            this.SablonKod = string.Empty;
            OnCreated();
        }
        public System.String? Aciklama { get; set; }

        [Key]
        public System.Int64 Id { get; set; }
        public System.String? Konu { get; set; }
        public System.String? Mesaj { get; set; }
        public System.String? Modul { get; set; }
        public System.String SablonKod { get; set; }
        public System.Byte SablonTip { get; set; }
    }
}
