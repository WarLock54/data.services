using System.ComponentModel.DataAnnotations;

namespace Model
{
    public partial class GeoResult
    {
        partial void OnCreated();
        public GeoResult() : base()
        {
            OnCreated();
        }
        [Key]
        public int Id { get; set; }
        public string Member { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public long Hash { get; set; }
        public string Unit { get; set; }
        public double Distance { get; set; }

        // Additional info
        public string EndUpdate { get; set; }
    }
}
