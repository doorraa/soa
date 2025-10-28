using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tours.API.Models
{
    public enum ExecutionStatus
    {
        Active,
        Completed,
        Abandoned
    }

    public class CompletedKeyPoint
    {
        public int KeyPointOrder { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class TourExecution
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public int TouristId { get; set; }
        public string TourId { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;

        public ExecutionStatus Status { get; set; } = ExecutionStatus.Active;

        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }

        public List<CompletedKeyPoint> CompletedKeyPoints { get; set; } = new List<CompletedKeyPoint>();

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? AbandonedAt { get; set; }
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    }
}
