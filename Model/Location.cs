using System.ComponentModel.DataAnnotations;

namespace Model
{
    public partial class Location
    {
        partial void OnCreated();
        public Location() : base()
        {
            OnCreated();
        }
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
