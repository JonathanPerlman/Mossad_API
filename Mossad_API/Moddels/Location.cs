using System.ComponentModel.DataAnnotations;

namespace Mossad_API.Moddels
{
    public class Location
    {
        [Key]
        public int ID { get; set; } 
        public Double x {  get; set; }
        public Double y { get; set; }  
    }
}
