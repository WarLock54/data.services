using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public partial class Market
    {
        partial void OnCreated();
        public Market() : base()
        {
            OnCreated();
        }
        [Key]
        public long Id { get; set; }
        public string BrandName { get; set; }
        public string Geolocation { get; set; }
    }
}
