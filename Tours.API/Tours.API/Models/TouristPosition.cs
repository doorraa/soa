using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tours.API.Models
{
    public class TouristPosition
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public int TouristId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
