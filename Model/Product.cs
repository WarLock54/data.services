using System.ComponentModel.DataAnnotations;

namespace Model
{
    public partial class Product
    {
        partial void OnCreated();
        public Product() : base()
        {
            OnCreated();
        }
        [Key]
        public long Id { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public long MarketId { get; set; }
        public virtual Market? Market { get; set; }
        public string Header { get; set; } = string.Empty;
    }
}
