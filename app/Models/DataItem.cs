using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArkitechDataApi.Models
{
    public class DataItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("topic")]
        public string Topic { get; set; } = null!;

        [BsonElement("payload")]
        public BsonDocument Payload { get; set; } = null!;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
