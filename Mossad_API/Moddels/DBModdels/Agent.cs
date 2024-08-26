using System.ComponentModel.DataAnnotations;
using Mossad_API.Moddels.Enums;

namespace Mossad_API.Moddels.DBModdels
{
    public class Agent
    {
        [Key]
        public int id { get; set; }
        public string photo_url { get; set; }
        public string nickname { get; set; }
        public Location? _Location { get; set; }
        public AgentStatus? Status { get; set; }

    }
}

