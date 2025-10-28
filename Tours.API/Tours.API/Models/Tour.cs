using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using static Tours.API.Models.Enums;

namespace Tours.API.Models
{
    public class Tour
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public int AuthorId { get; set; } // ID autora iz Stakeholders.API

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public TourDifficulty Difficulty { get; set; }
        public TourStatus Status { get; set; } = TourStatus.Draft;

        public decimal Price { get; set; } = 0;

        public List<string> Tags { get; set; } = new List<string>();
        public List<KeyPoint> KeyPoints { get; set; } = new List<KeyPoint>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }

        // Helper properties
        public KeyPoint? StartPoint => KeyPoints.OrderBy(kp => kp.Order).FirstOrDefault();
        public KeyPoint? EndPoint => KeyPoints.OrderByDescending(kp => kp.Order).FirstOrDefault();
        public double DurationHours { get; set; }
    }
}
