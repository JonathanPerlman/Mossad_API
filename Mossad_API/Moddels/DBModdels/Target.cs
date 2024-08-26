using System.ComponentModel.DataAnnotations;
using Mossad_API.Moddels.Enums;

namespace Mossad_API.Moddels.DBModdels
{
    public class Target
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string position { get; set; }
        public string photo_url { get; set; }
        public Location? _Location { get; set; }

        public TargetStatus? Status { get; set; }

    }
}
