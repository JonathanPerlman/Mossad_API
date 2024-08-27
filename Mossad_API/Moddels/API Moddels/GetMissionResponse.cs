using Mossad_API.Moddels.Enums;

namespace Mossad_API.Moddels.API_Moddels
{
    public class GetMissionResponse
    {
        public int Id { get; set; }
        public string AgentName { get; set; }
        public Double AgentX { get; set; }
        public Double AgentY { get; set; }
        public string TargetName { get; set; }
        public Double TargetX { get; set; }
        public Double TaregtY { get; set; }
        public Double Distance { get; set; }
        public MissionStatus? Status { get; set; }
        public Double? TimeLeft { get; set; }
    }
}


