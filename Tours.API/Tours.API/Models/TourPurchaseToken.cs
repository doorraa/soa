using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tours.API.Models
{
    public class TourPurchaseToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public int TouristId { get; set; }
        public string TourId { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public decimal PricePaid { get; set; }
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    }
}
