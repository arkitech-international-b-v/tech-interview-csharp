namespace ArkitechDataApi.Models
{
    public class MqttSettings
    {
        public string BrokerHost { get; set; } = null!;
        public int BrokerPort { get; set; }
        public string TopicFilter { get; set; } = null!;
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
