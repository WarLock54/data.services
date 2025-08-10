using System.ComponentModel.DataAnnotations;

namespace Model
{
    public partial class FindGeo
    {
        partial void OnCreated();
        public FindGeo() : base()
        {
            OnCreated();
        }
        [Key]
        public int Id { get; set; }
        public string Country { get; set; }
        public double WithinKm { get; set; }
        public double Lng { get; set; }
        public double Lat { get; set; }
    }
}
