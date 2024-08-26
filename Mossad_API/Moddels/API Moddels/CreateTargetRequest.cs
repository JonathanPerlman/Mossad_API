namespace Mossad_API.Moddels
{
    public class CreateTargetRequest
    {
        public string name { get; set; }
        public string? position { get; set; }    
        public string photoUrl { get; set; }
        public string? token { get; set; }
    }
}
