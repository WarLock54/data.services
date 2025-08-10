using System.ComponentModel.DataAnnotations;

namespace Model
{
    public partial class LocationDate
    {
        partial void OnCreated();
        public LocationDate() : base()
        {
            OnCreated();
        }
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime EndDate { get; set; }
    }
}
