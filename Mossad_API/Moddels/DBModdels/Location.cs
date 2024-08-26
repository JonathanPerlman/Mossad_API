using System.ComponentModel.DataAnnotations;

namespace Mossad_API.Moddels.DBModdels
{
    public class Location
    {
        [Key]
        public int ID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}
