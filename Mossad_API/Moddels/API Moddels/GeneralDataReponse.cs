namespace Mossad_API.Moddels.API_Moddels
{
    public class GeneralDataReponse
    {
        public int AgentsCount { get; set; }
        public int AgentsInActivityCount { get; set; }
        public int TargetsCount { get; set; }
        public int TargetsKilledCount { get; set; }
        public int MissionsCount { get; set; }
        public int MissionsAssignedCount { get; set; }
        public Double RelationOfAgentsToTargets { get; set; }
        public Double RelationOfAvaliableAgentsToTargets { get; set; }
    }
}
