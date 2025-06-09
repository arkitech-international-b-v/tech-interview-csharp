namespace ArkitechDataApi.Models
{
    public class LatestDataDto
    {
        public string Topic { get; set; } = null!;          
        public DateTime Timestamp { get; set; }             
        public Dictionary<string, object> Payload { get; set; } = new();

    }
}
