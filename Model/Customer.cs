using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public partial class Customer
    {
        partial void OnCreated();

        public Customer() : base()
        {
            this.Customers= new List<Customer>();
            this.Ad = string.Empty;
            this.AktifEh = string.Empty;
            this.CustomerTuru = string.Empty;
            OnCreated();
        }
        public System.String Ad { get; set; }
        public System.String AktifEh { get; set; }
        public Nullable<System.Int64> AnaCustomerKod { get; set; }
        public System.String? Adres { get; set; }
        public System.String? EPosta { get; set; }
        public System.String CustomerTuru { get; set; }
        public Nullable<System.Int32> FaxAlanKod { get; set; }
        public System.String? FaxNo { get; set; }
   
        public System.Int64 KimlikNo { get; set; }
        public System.String? KisaAd { get; set; }

        [Key]
        public System.Int64 Id { get; set; }
        public System.String? Notlar { get; set; }
        public Nullable<System.Byte> Seviye { get; set; }
        public Nullable<System.Int32> TelAlanKod { get; set; }
        public System.String? TelNo { get; set; }
        public System.String? YetkiliAdSoyad { get; set; }
        public Nullable<System.Int64> YetkiliCustomerKod { get; set; }
        public System.String? YetkiliTelNo { get; set; }
        public System.String? YetkiliUnvani { get; set; }
        public virtual Customer? AnaCustomer { get; set; }
        public virtual List<Customer> Customers { get; set; }
    }
}
