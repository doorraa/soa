using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tours.API.Models
{
    public class OrderItem
    {
        public string TourId { get; set; } = string.Empty;
        public string TourName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class ShoppingCart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public int TouristId { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalPrice { get; set; } = 0;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
